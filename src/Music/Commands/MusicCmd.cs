using Lagrange.Core.Message;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Music.Commands;

public class MusicCmd : Command
{
    public override string HelpText => "���";
    public override string[] Alias => ["���"];
    public override string[] Permissions => [OneBotPermissions.Music];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count > 0)
        {
            var musicName = string.Join(" ", args.Parameters);
            if (args.Parameters[0] == "����")
            {
                if (args.Parameters.Count > 1)
                {
                    await args.Event.Reply(MessageBuilder.Group(args.Event.Chain.GroupUin!.Value).MarkdownImage(await MusicTool.GetMusic163Markdown(musicName[2..])));
                    MusicTool.ChangeName(musicName[2..], args.Event.Chain.GroupMemberInfo!.Uin);
                    MusicTool.ChangeLocal("����", args.Event.Chain.GroupMemberInfo!.Uin);
                }
                else
                {
                    await args.Event.Reply("������һ������!");
                }
            }
            else if (args.Parameters[0] == "QQ")
            {
                if (args.Parameters.Count > 1)
                {
                    await args.Event.Reply(MessageBuilder.Group(args.Event.Chain.GroupUin!.Value).MarkdownImage(await MusicTool.GetMusicQQMarkdown(musicName[2..])));
                    MusicTool.ChangeName(musicName[2..], args.Event.Chain.GroupMemberInfo!.Uin);
                    MusicTool.ChangeLocal("QQ", args.Event.Chain.GroupMemberInfo!.Uin);
                }
                else
                {
                    await args.Event.Reply("������һ������!");
                }
            }
            else
            {
                var type = MusicTool.GetLocal(args.Event.Chain.GroupMemberInfo!.Uin);
                if (type == "����")
                {
                    try
                    {
                        await args.Event.Reply(MessageBuilder.Group(args.Event.Chain.GroupUin!.Value).MarkdownImage(await MusicTool.GetMusic163Markdown(musicName)));
                    }
                    catch (Exception ex)
                    {
                        await args.Event.Reply(ex.Message);
                    }
                    MusicTool.ChangeName(musicName, args.Event.Chain.GroupMemberInfo!.Uin);
                }
                else
                {
                    await args.Event.Reply(MessageBuilder.Group(args.Event.Chain.GroupUin!.Value).MarkdownImage(await MusicTool.GetMusicQQMarkdown(musicName)));
                    MusicTool.ChangeName(musicName, args.Event.Chain.GroupMemberInfo!.Uin);
                }
            }
        }
        else
        {
            await args.Event.Reply("������һ������!");
        }
    }
}
