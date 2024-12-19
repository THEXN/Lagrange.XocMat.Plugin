﻿using Lagrange.Core;
using Lagrange.Core.Message;
using Lagrange.XocMat.Commands;
using Lagrange.XocMat.Configuration;
using Lagrange.XocMat.DB.Manager;
using Lagrange.XocMat.Extensions;
using Lagrange.XocMat.Permission;
using Lagrange.XocMat.Plugin;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text;

namespace TerrariaCart;

public class Plugin(ILogger logger, CommandManager commandManager, BotContext bot) : XocMatPlugin(logger, commandManager, bot)
{
    public override string Name => "TerrariaCart";

    public override string Description => "提供泰拉商店的购物车功能，更加方便的使用商店!";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);

    public Config Config { get; set; } = new();


    public override void Initialize()
    {
        CommandManager.AddGroupCommand(new("cart", CartManager, OneBotPermissions.TerrariaShop));
        CommandManager.AddServerCommand(new("结算", CartBuy, OneBotPermissions.TerrariaShop));
    }

    public async ValueTask CartBuy(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        if (args.Parameters.Count != 1)
        {
            await args.Server.PrivateMsg(args.UserName, $"语法错误:\n正确语法:/结算 [购物车]", Color.GreenYellow);
            return;
        }
        if (!args.Server.EnabledShop)
        {
            await args.Server.PrivateMsg(args.UserName, "服务器未开启商店系统！", Color.DarkRed);
            return;
        }
        if (args.User != null)
        {
            try
            {
                var carts = Config.GetCartShop(args.Account.UserId, args.Parameters[0]);
                if (carts.Count == 0)
                {
                    await args.Server.PrivateMsg(args.UserName, "购物车中不存在物品!", Color.DarkRed);
                    return;
                }
                var all = carts.Sum(x => x.Price);
                var curr = Currency.Query(args.User.GroupID, args.User.Id);
                if (curr != null && curr.Num >= all)
                {
                    foreach (var shop in carts)
                    {
                        var res = await args.Server.Command($"/g {shop.ID} {args.Name} {shop.Num}");
                    }
                    await args.Server.PrivateMsg(args.UserName, "结算成功!", Color.GreenYellow);
                }
                else
                {
                    await args.Server.PrivateMsg(args.UserName, "星币不足!", Color.GreenYellow);
                }
            }
            catch (Exception e)
            {
                await args.Server.PrivateMsg(args.UserName, e.Message, Color.DarkRed);
                return;
            }

        }
    }

    private async ValueTask CartManager(CommandArgs args)
    {
        try
        {
            if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "add")
            {
                if (int.TryParse(args.Parameters[2], out int id))
                {
                    Config.Add(args.EventArgs.Chain.GroupMemberInfo!.Uin, args.Parameters[1], id);
                    await args.EventArgs.Reply("添加成功!", true);
                }
                else
                {
                    await args.EventArgs.Reply("请填写一个正确的商品ID!", true);
                }
            }
            else if (args.Parameters.Count == 3 && args.Parameters[0].ToLower() == "del")
            {
                if (int.TryParse(args.Parameters[2], out int id))
                {
                    Config.Remove(args.EventArgs.Chain.GroupMemberInfo!.Uin, args.Parameters[1], id);
                    await args.EventArgs.Reply("删除成功!", true);
                }
                else
                {
                    await args.EventArgs.Reply("请填写一个正确的商品ID!", true);
                }
            }
            else if (args.Parameters.Count == 2 && args.Parameters[0].ToLower() == "clear")
            {
                Config.ClearCart(args.EventArgs.Chain.GroupMemberInfo!.Uin, args.Parameters[1]);
                await args.EventArgs.Reply("已清除购物车" + args.Parameters[1]);
            }
            else if (args.Parameters.Count == 1 && args.Parameters[0].ToLower() == "list")
            {
                var carts = Config.GetCarts(args.EventArgs.Chain.GroupMemberInfo!.Uin);
                if (carts.Count == 0)
                {
                    await args.EventArgs.Reply("购物车空空如也!", true);
                    return;
                }
                var sb = new StringBuilder();
                sb.AppendLine($$"""<div align="center">""");
                sb.AppendLine();
                sb.AppendLine();
                foreach (var (name, shops) in carts)
                {
                    sb.AppendLine();
                    sb.AppendLine($"# 购物车`{name}`");
                    sb.AppendLine();
                    sb.AppendLine("|商品ID|商品名称|数量|价格|");
                    sb.AppendLine("|:--:|:--:|:--:|:--:|");
                    foreach (var index in shops)
                    {
                        var shop = TerrariaShop.Instance.GetShop(index);
                        if (shop != null)
                            sb.AppendLine($"|{index}|{shop.Name}|{shop.Num}|{shop.Price}|");
                    }
                }
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("</div>");

                await args.EventArgs.Reply(MessageBuilder.Group(args.EventArgs.Chain.GroupUin!.Value).MarkdownImage(sb.ToString()));
            }
            else
            {
                await args.EventArgs.Reply("语法错误,正确语法\n" +
                    $"{args.CommamdPrefix}{args.Name} add [购物车] [商品ID]\n" +
                    $"{args.CommamdPrefix}{args.Name} del [购物车] [商品ID]\n" +
                    $"{args.CommamdPrefix}{args.Name} clear [购物车]\n" +
                    $"{args.CommamdPrefix}{args.Name} list");
            }

        }
        catch (Exception e)
        {
            await args.EventArgs.Reply(e.Message);
        }
        Config.Save();
    }

    protected override void Dispose(bool dispose)
    {
        CommandManager.GroupCommandDelegate.RemoveAll(x => x.CallBack == CartManager);
        CommandManager.ServerCommandDelegate.RemoveAll(x => x.CallBack == CartBuy);
    }
}
