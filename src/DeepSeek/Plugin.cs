﻿using Lagrange.Core;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Plugin;
using Microsoft.Extensions.Logging;

namespace DeepSeek;

public class Plugin(ILogger logger, BotContext bot) : XocMatPlugin(logger, bot)
{
    public override string Name => "DeepSeek";

    public override string Description => "AI对话插件";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);

    protected override void Initialize()
    {
        BotContext.Invoker.OnGroupMessageReceived += Invoker_OnGroupMessageReceived;
    }

    private void Invoker_OnGroupMessageReceived(BotContext context, Lagrange.Core.Event.EventArg.GroupMessageEvent e)
    {
        if (e.Chain.GetMention().Any(x => x.Uin == context.BotUin))
        {
            Task.Factory.StartNew(async () =>
            {
                if (e.Chain.GetMention().Any(x => x.Uin == context.BotUin))
                {
                    var text = e.Chain.GetText();
                    if (string.IsNullOrEmpty(text))
                    {
                        await e.Reply("内容不能为空", true);
                        return;
                    }
                    try
                    {

                        var res = Config.Instance.UseContext ? await Utils.Instance.ChatContent(e.Chain.GroupMemberInfo!.Uin, text) : await Utils.Instance.Chat(text);
                        var builder = MessageBuilder.Group(e.Chain.GroupUin!.Value)
                            .MultiMsg(MessageBuilder.Friend(e.Chain.GroupMemberInfo!.Uin).MultiMsg(MessageBuilder.Friend(e.Chain.GroupMemberInfo!.Uin).Markdown(new MarkdownData() { Content = res })));
                        await e.Reply(builder);

                    }
                    catch (Exception ex)
                    {
                        await e.Reply(ex.Message, true);
                    }
                }
            });
        }
    }

    protected override void Dispose(bool dispose)
    {
        BotContext.Invoker.OnGroupMessageReceived -= Invoker_OnGroupMessageReceived;
    }
}
