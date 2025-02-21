using System.Text;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;

namespace OnlineReward;

public class CReward : Command
{
    public override string HelpText => "��ȡ����ʱ������";
    public override string[] Alias => ["��ȡ���߽���"];
    public override string[] Permissions => ["onebot.online.reward"];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out var server) && server != null)
        {
            var user = TerrariaUser.GetUserById(args.MemberUin, server.Name);
            if (user.Count == 0)
            {
                await args.Event.Reply("δ�ҵ�ע����˻�", true);
                return;
            }
            var online = await server.OnlineRank();
            if (!online.Status)
            {
                await args.Event.Reply(online.Message, true);
                return;
            }
            var sb = new StringBuilder();
            foreach (var u in user)
            {
                if (online.OnlineRank.TryGetValue(u.Name, out var time))
                {
                    Config.Instance.Reward.TryGetValue(u.Name, out int ctime);
                    var ntime = time - ctime;
                    if (ntime > 0)
                    {
                        Config.Instance.Reward[u.Name] = time;
                        sb.AppendLine($"��ɫ: {u.Name}����ʱ��{time}��,������ȡ{ntime}�뽱������{ntime * Config.Instance.TimeRate}���Ǳ�!");
                        Currency.Add(args.MemberUin, ntime * Config.Instance.TimeRate);
                    }
                    else
                    {
                        sb.AppendLine($"��ɫ: {u.Name}������ʱ�������޷���ȡ");
                    }
                }
                else
                {
                    sb.AppendLine($"��ɫ: {u.Name}������ʱ�������޷���ȡ");
                }
            }
            await args.Event.Reply(sb.ToString().Trim());
            Config.Save();
        }
        else
        {
            await args.Event.Reply("���л���һ����Ч�ķ�����!", true);
            return;
        }
    }
}
