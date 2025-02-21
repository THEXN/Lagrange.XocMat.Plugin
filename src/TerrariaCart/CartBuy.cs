using System.Drawing;
using Lagrange.XocMat.Command;
using Lagrange.XocMat.Command.CommandArgs;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Internal;

namespace TerrariaCart;

public class CartBuy : Command
{
    private readonly Config _config;

    public CartBuy()
    {
        _config = new Config();
    }

    public override string HelpText => "���㹺�ﳵ";
    public override string[] Alias => ["����"];
    public override string[] Permissions => [OneBotPermissions.TerrariaShop];

    public override async Task InvokeAsync(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        if (args.Parameters.Count != 1)
        {
            await args.Server.PrivateMsg(args.UserName, $"�﷨����:\n��ȷ�﷨:/���� [���ﳵ]", Color.GreenYellow);
            return;
        }
        if (!args.Server.EnabledShop)
        {
            await args.Server.PrivateMsg(args.UserName, "������δ�����̵�ϵͳ��", Color.DarkRed);
            return;
        }
        if (args.User != null)
        {
            try
            {
                var carts = _config.GetCartShop(args.Account.UserId, args.Parameters[0]);
                if (carts.Count == 0)
                {
                    await args.Server.PrivateMsg(args.UserName, "���ﳵ�в�������Ʒ!", Color.DarkRed);
                    return;
                }
                var all = carts.Sum(x => x.Price);
                var curr = Currency.Query(args.User.Id);
                if (curr != null && curr.Num >= all)
                {
                    foreach (var shop in carts)
                    {
                        var res = await args.Server.Command($"/g {shop.ID} {args.Name} {shop.Num}");
                    }
                    await args.Server.PrivateMsg(args.UserName, "����ɹ�!", Color.GreenYellow);
                }
                else
                {
                    await args.Server.PrivateMsg(args.UserName, "�ǱҲ���!", Color.GreenYellow);
                }
            }
            catch (Exception e)
            {
                await args.Server.PrivateMsg(args.UserName, e.Message, Color.DarkRed);
                return;
            }
        }
    }
}

