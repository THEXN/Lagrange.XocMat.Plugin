using Lagrange.Core.Common.Interface.Api;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;
using Music.WangYi;

namespace Music.Commands;

public class DownloadPlay : Command
{
    public override string HelpText => "���ظ赥";
    public override string[] Alias => ["���ظ赥"];
    public override string[] Permissions => [OneBotPermissions.Music];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (long.TryParse(args.Parameters[0], out var id))
        {
            try
            {
                await args.Event.Reply("�������ظ赥�еĸ���...");
                var source = MusicTool.GetLocal(args.Event.Chain.GroupMemberInfo!.Uin);
                var buffer = source switch
                {
                    "QQ" => await Config.Instance.MusicQQ.DownloadPlaylists(id),
                    "����" => await Music_163.DownloadPlaylists(id),
                    _ => throw new("δ֪������Դ")
                };
                await args.Bot.GroupFSUpload(args.Event.Chain.GroupUin!.Value, new(buffer, $"{source}�赥[{id}].zip"));
            }
            catch (Exception ex)
            {
                await args.Event.Reply(ex.Message);
            }
        }
        else
        {
            await args.Event.Reply("������һ����ȷ�ĸ赥ID!");
        }
    }
}
