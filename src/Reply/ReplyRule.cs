﻿using System.Text.RegularExpressions;
using Lagrange.Core.Message;
using Lagrange.XocMat.Extensions;
using Newtonsoft.Json;

namespace Reply;

public enum ContentType { Text, Image, Face, Video, At, Forward, SelectAt }

public class ContentItem(ContentType type, string content)
{
    public ContentType Type { get; set; } = type;
    public string Content { get; set; } = content;
}

public class Response
{
    public List<ContentItem> Contents { get; set; } = new List<ContentItem>();
}

public class ReplyRule
{
    public string MatchPattern { get; set; }
    
    [JsonIgnore]
    public Regex TriggerRegex { get; set; }
    public string ReplyTemplate { get; set; }

    public ReplyRule(string matchPattern, string replyTemplate)
    {
        MatchPattern = matchPattern;
        ReplyTemplate = replyTemplate;
        TriggerRegex = new Regex(matchPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}

public delegate Task<string> AsyncVariableHandler(string varName, string? param, MessageChain chain);
public delegate Task ContentTypeHandler(string type, string content, MessageChain chain, MessageBuilder builder);

public class ReplyAdapter
{
    private static List<ReplyRule> _rules => Config.Instance.Rules;
    private static readonly Dictionary<string, AsyncVariableHandler> _asyncHandlers = [];
    private static readonly Dictionary<string, ContentTypeHandler> _contentHandlers = [];
    
    public static Action<string> Logger { get; set; } = Console.WriteLine;

    public static void RegisterAsyncHandler(string varName, AsyncVariableHandler handler)
    {
        _asyncHandlers[varName.ToLower()] = handler;
    }

    public static void RemoveAsyncHandler(string varName)
    {
        _asyncHandlers.Remove(varName.ToLower());
    }

    public static void RemoveContentHandler(string contentType)
    {
        _contentHandlers.Remove(contentType.ToLower());
    }

    public static void RegisterContentHandler(string contentType, ContentTypeHandler handler)
    {
        _contentHandlers[contentType.ToLower()] = handler;
    }

    public static List<string> GetVariables() => [.. _asyncHandlers.Keys];
    public static List<string> GetContentHandlers() => [.. _contentHandlers.Keys];

    public static async Task<MessageBuilder?> ProcessMessageAsync(MessageChain chain)
    {
        var message = chain.GetText().Trim();
        foreach (var rule in _rules)
        {
            Logger($"测试规则: {rule.MatchPattern}");
            
            var match = rule.TriggerRegex.Match(message);
            if (!match.Success) continue;

            Logger($"匹配成功，分组数: {match.Groups.Count}");
            for (int i = 0; i < match.Groups.Count; i++)
            {
                Logger($"Group[{i}]: {match.Groups[i].Value}");
            }

            var processed = await ProcessTemplateAsync(match, rule.ReplyTemplate, chain);
            return await BuildResponseAsync(processed, chain);
        }
        return null;
    }

    private static async Task<string> ProcessTemplateAsync(Match match, string template, MessageChain chain)
    {
        var step1 = await ReplaceVariablesAsync(template, chain);
        return ReplaceRegexGroups(match, step1);
    }

    private static string ReplaceRegexGroups(Match match, string input)
    {
        return Regex.Replace(input, @"\$(\d+)", m =>
        {
            if (!int.TryParse(m.Groups[1].Value, out int index)) return m.Value;
            
            if (index <= 0 || index >= match.Groups.Count)
            {
                Logger($"无效分组索引: ${index}");
                return m.Value;
            }
            
            var value = match.Groups[index].Value;
            Logger($"替换分组: ${index} → {value}");
            return value;
        });
    }

    private static async Task<string> ReplaceVariablesAsync(string input, MessageChain chain)
    {
        var pattern = @"\$(\w+)(?::([^$]+))?";
        var replacements = new Dictionary<string, string>();

        foreach (Match m in Regex.Matches(input, pattern))
        {
            var varName = m.Groups[1].Value.ToLower();
            var parameter = m.Groups[2].Success ? m.Groups[2].Value : null;
            
            if (_asyncHandlers.TryGetValue(varName, out var handler))
            {
                try
                {
                    var value = await handler(varName, parameter, chain);
                    replacements[m.Value] = value;
                }
                catch (Exception ex)
                {
                    Logger($"处理变量失败: {varName} - {ex.Message}");
                    replacements[m.Value] = $"[{varName}错误]";
                }
            }
        }

        return Regex.Replace(input, pattern, m => 
            replacements.GetValueOrDefault(m.Value, m.Value));
    }

    private static async Task<MessageBuilder> BuildResponseAsync(string processed, MessageChain chain)
    {
        var pattern = @"{(?<type>\w+):\s*(?<content>[^}]+?)\s*}|(?<text>[^{]*)";
        var builder = MessageBuilder.Group(chain.GroupUin!.Value);
        foreach (Match m in Regex.Matches(processed, pattern))
        {
            if (m.Groups["type"].Success)
            {
                var type = m.Groups["type"].Value.ToLower();
                var content = m.Groups["content"].Value.Trim();
                
                // 调用自定义内容处理器
                if (_contentHandlers.TryGetValue(type, out var handler))
                {
                    try
                    {
                        await handler(type, content, chain, builder);
                    }
                    catch (Exception ex)
                    {
                        Logger($"处理内容失败: {type} - {ex.Message}");
                    }
                }
            }
            else
            {
                builder.Text(m.Groups["text"].Value.Trim());
            }
        }

        return builder;
    }
}