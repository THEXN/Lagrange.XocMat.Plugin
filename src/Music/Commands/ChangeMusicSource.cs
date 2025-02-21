using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace Music.Commands;

public class ChangeMusicSource : Command
{
    public override string HelpText => "�л�����Դ";
    public override string[] Alias => ["�л���Դ"];
    public override string[] Permissions => [OneBotPermissions.Music];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (args.Parameters.Count > 0)
        {
            if (args.Parameters[0] == "QQ" || args.Parameters[0] == "����")
            {
                MusicTool.ChangeLocal(args.Parameters[0], args.Event.Chain.GroupMemberInfo!.Uin);
                await args.Event.Reply($"��Դ���л���{args.Parameters[0]}");
            }
            else
            {
                await args.Event.Reply("��������ȷ����Դ!");
            }
        }
        else
        {
            await args.Event.Reply("��������ȷ����Դ!");
        }
    }
}
