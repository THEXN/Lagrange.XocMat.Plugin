using System.Text;
using Lagrange.Core.Message;
using Lagrange.XocMat;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace PluginManager;

public class PManager : Command
{
    private readonly PluginLoader _loader;

    public PManager()
    {
        _loader = XocMatApp.Instance.Services.GetRequiredService<PluginLoader>();
    }

    public override string HelpText => "�������";
    public override string[] Alias => ["pm"];
    public override string[] Permissions => ["onebot.plugin.admin"];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count == 1 && args.Parameters[0].Equals("list", StringComparison.CurrentCultureIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine($$"""<div align="center">""");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine($"# ����б�");
            sb.AppendLine();
            sb.AppendLine("|���|�������|�������|���˵��|����汾|����|");
            sb.AppendLine("|:--:|:--:|:--:|:--:|:--:|:--:|");
            int index = 1;
            foreach (var plugin in _loader.PluginContext.Plugins)
            {
                sb.AppendLine($"|{index}|{plugin.Plugin.Name}|{plugin.Plugin.Author}|{plugin.Plugin.Description}|{plugin.Plugin.Version}|{plugin.Initialized}|");
                index++;
            }
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("</div>");
            await args.Event.Reply(MessageBuilder.Group(args.Event.Chain.GroupUin!.Value).MarkdownImage(sb.ToString()));
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "off")
        {
            if (!int.TryParse(args.Parameters[1], out var index) || index < 1 || index > _loader.PluginContext.Plugins.Count)
            {
                await args.Event.Reply("������һ����ȷ�����!", true);
                return;
            }
            var instance = _loader.PluginContext.Plugins[index - 1];
            if (!instance.Initialized)
            {
                await args.Event.Reply("�˲���Ѿ���ж�أ������ظ�ж��!!", true);
                return;
            }
            instance.DeInitialize();
            await args.Event.Reply($"{instance.Plugin.Name} ���ж�سɹ�!", true);
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "on")
        {
            if (!int.TryParse(args.Parameters[1], out var index) || index < 1 || index > _loader.PluginContext.Plugins.Count)
            {
                await args.Event.Reply("������һ����ȷ�����!", true);
                return;
            }
            var instance = _loader.PluginContext.Plugins[index - 1];
            if (instance.Initialized)
            {
                await args.Event.Reply("�˲���Ѿ������ã������ظ�����!!", true);
                return;
            }
            instance.Initialize();
            await args.Event.Reply($"{instance.Plugin.Name} ������سɹ�!", true);
        }
        else if (args.Parameters.Count == 1 && args.Parameters[0].ToLower() == "reload")
        {
            _loader.UnLoad();
            _loader.Load();
            await args.Event.Reply("����б��Ѿ����¼���!", true);
        }
        else
        {
            await args.Event.Reply("�﷨����,��ȷ�﷨:\n" +
                $"{args.CommamdPrefix}{args.Name} list" +
                $"{args.CommamdPrefix}{args.Name} off [���]" +
                $"{args.CommamdPrefix}{args.Name} on [���]");
        }
    }
}
