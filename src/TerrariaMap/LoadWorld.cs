using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Message.Entity;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Internal;

namespace TerrariaMap;

public class LoadWorld : Command
{
    public override string HelpText => "��ȡTerraria��ͼ";
    public override string[] Alias => ["��ȡ��ͼ"];
    public override string[] Permissions => [OneBotPermissions.GenerateMap];

    public override async Task InvokeAsync(GroupCommandArgs args)
    {
        if (UserLocation.Instance.TryGetServer(args.MemberUin, args.GroupUin, out var server) && server != null)
        {
            var file = await server.GetWorldFile();
            if (file.Status)
            {
                await args.Bot.GroupFSUpload(args.Event.Chain.GroupUin!.Value, new FileEntity(file.WorldBuffer, file.WorldName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".wld"));
            }
            else
            {
                await args.Event.Reply("�޷����ӵ�������!");
            }
        }
        else
        {
            await args.Event.Reply("���л���һ����Ч�ķ�����!");
        }
    }
}


