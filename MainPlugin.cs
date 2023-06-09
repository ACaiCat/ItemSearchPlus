﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using System.Threading;
using Terraria.ID;

[ApiVersion(2, 1)]
public class MainPlugin : TerrariaPlugin
{
    QueryResult queryResult = null;

    Dictionary<int, string> data = new Dictionary<int, string>();
    public override string Name => "物品查找";

    public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    public override string Author => "棱镜 & Cai优化升级";

    public override string Description => "显示拥有指定物品的玩家或箱子";

    public MainPlugin(Main game)
        : base(game)
    {
    }

    public override void Initialize()
    {
        Commands.ChatCommands.Add(new Command("itemsearch.cmd", ItemSearchCmd, "searchitem", "si", "查找物品"));
        Commands.ChatCommands.Add(new Command("itemsearch.chest", ChestSearchCmd, "searchchest", "sc", "查找箱子"));
        Commands.ChatCommands.Add(new Command("itemsearch.chesttp", TpSearchCmd, "tpchest", "tpc", "传送箱子"));
        Commands.ChatCommands.Add(new Command("itemsearch.chestinfo", InfoSearchCmd, "chestinfo", "ci", "箱子信息"));
        Commands.ChatCommands.Add(new Command("itemsearch.tpall", TpAllPlayer, "tpall", "传送所有人"));
        Commands.ChatCommands.Add(new Command("itemsearch.tpall", TpAllChest, "tpallchest", "tpallc", "传送所有箱子"));
        Commands.ChatCommands.Add(new Command("itemsearch.rci", RemoveItemChest, "removechestitem", "rci", "删除箱子物品"));
        Commands.ChatCommands.Add(new Command("itemsearch.ri", RemoveItem, "removeitem", "ri", "删除物品"));
    }

    private void RemoveItem(CommandArgs args)
    {
        data.Clear();
        if (args.Parameters.Count != 2)
        {
            args.Player.SendInfoMessage("用法:/ri <玩家名> <物品名/ID>");
            return;
        }
        var acc = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
        if (acc == null)
        {
            args.Player.SendErrorMessage($"找不到名字为{args.Parameters[0]}的玩家!");
            return;
        }

        List<Item> itemByIdOrName = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
        if (itemByIdOrName.Count > 1)
        {
            args.Player.SendMultipleMatchError(from i in itemByIdOrName
                                               select (args.Player.RealPlayer ? string.Format("[i:{0}]", i.type) : "") + i.Name);
            return;
        }
        if (itemByIdOrName.Count == 0)
        {
            args.Player.SendErrorMessage("指定的物品无效");
            return;
        }
        int item = itemByIdOrName[0].netID;
        TSPlayer player = new(-1);
        PlayerData? playerdata;
        int count = 0;
        if (TSPlayer.FindByNameOrID(acc.Name).FirstOrDefault() != null)
        {
            //Item[] armor;
            //Item[] dye;
            //Item[] miscEquips;
            //Item[] miscDyes;
            var plr = TSPlayer.FindByNameOrID(acc.Name).FirstOrDefault()!;
            for (int i = 0; i < plr.TPlayer.inventory.Length; i++)
            {
                if (plr.TPlayer.inventory[i].netID == item)
                {
                    count += plr.TPlayer.inventory[i].stack;
                    plr.TPlayer.inventory[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, i);

                }
            }
            for (int i = 0; i < plr.TPlayer.armor.Length; i++)
            {
                if (plr.TPlayer.armor[i].netID == item)
                {
                    count += plr.TPlayer.armor[i].stack;
                    plr.TPlayer.armor[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Armor0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.dye.Length; i++)
            {
                if (plr.TPlayer.dye[i].netID == item)
                {
                    count += plr.TPlayer.dye[i].stack;
                    plr.TPlayer.dye[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Dye0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.miscEquips.Length; i++)
            {
                if (plr.TPlayer.miscEquips[i].netID == item)
                {
                    count += plr.TPlayer.miscEquips[i].stack;
                    plr.TPlayer.miscEquips[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Misc0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.miscDyes.Length; i++)
            {
                if (plr.TPlayer.miscDyes[i].netID == item)
                {
                    count += plr.TPlayer.miscEquips[i].stack;
                    plr.TPlayer.miscEquips[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.MiscDye0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank.item.Length; i++)
            {
                if (plr.TPlayer.bank.item[i].netID == item)
                {
                    count += plr.TPlayer.bank.item[i].stack;
                    plr.TPlayer.bank.item[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank1_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank2.item.Length; i++)
            {
                if (plr.TPlayer.bank2.item[i].netID == item)
                {
                    count += plr.TPlayer.bank2.item[i].stack;
                    plr.TPlayer.bank2.item[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank2_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank3.item.Length; i++)
            {
                if (plr.TPlayer.bank3.item[i].netID == item)
                {
                    count += plr.TPlayer.bank3.item[i].stack;
                    plr.TPlayer.bank3.item[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank3_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank4.item.Length; i++)
            {
                if (plr.TPlayer.bank4.item[i].netID == item)
                {
                    count += plr.TPlayer.bank4.item[i].stack;
                    plr.TPlayer.bank4.item[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank4_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[0].Armor.Length; i++)
            {
                if (plr.TPlayer.Loadouts[0].Armor[i].netID == item)
                {
                    count += plr.TPlayer.Loadouts[0].Armor[i].stack;
                    plr.TPlayer.Loadouts[0].Armor[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout1_Armor_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[1].Armor.Length; i++)
            {
                if (plr.TPlayer.Loadouts[1].Armor[i].netID == item)
                {
                    count += plr.TPlayer.Loadouts[1].Armor[i].stack;
                    plr.TPlayer.Loadouts[1].Armor[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout2_Armor_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[2].Armor.Length; i++)
            {
                if (plr.TPlayer.Loadouts[2].Armor[i].netID == item)
                {
                    count += plr.TPlayer.Loadouts[2].Armor[i].stack;
                    plr.TPlayer.Loadouts[2].Armor[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout3_Armor_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[0].Dye.Length; i++)
            {
                if (plr.TPlayer.Loadouts[0].Dye[i].netID == item)
                {
                    count += plr.TPlayer.Loadouts[0].Dye[i].stack;

                    plr.TPlayer.Loadouts[0].Dye[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout1_Dye_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[1].Dye.Length; i++)
            {
                if (plr.TPlayer.Loadouts[1].Dye[i].netID == item)
                {
                    count += plr.TPlayer.Loadouts[1].Dye[i].stack;
                    plr.TPlayer.Loadouts[1].Dye[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout2_Dye_0 + i);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[2].Dye.Length; i++)
            {
                if (plr.TPlayer.Loadouts[2].Dye[i].netID == item)
                {
                    count += plr.TPlayer.Loadouts[2].Dye[i].stack;
                    plr.TPlayer.Loadouts[2].Dye[i].SetDefaults(0);
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout3_Dye_0 + i);

                }
            }
            if (plr.TPlayer.trashItem.netID == item)
            {
                count += plr.TPlayer.trashItem.stack;
                plr.TPlayer.trashItem.SetDefaults(0);
                NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.TrashItem);
            }
            //Item[] array = player.TPlayer.inventory; 1
            //Item[] armor = player.TPlayer.armor;1
            //Item[] dye = player.TPlayer.dye;1
            //Item[] miscEquips = player.TPlayer.miscEquips;1
            //Item[] miscDyes = player.TPlayer.miscDyes;1
            //Item[] item = player.TPlayer.bank.item;1
            //Item[] item2 = player.TPlayer.bank2.item;1
            //Item[] item3 = player.TPlayer.bank3.item;1
            //Item[] item4 = player.TPlayer.bank4.item;1
            //Item trashItem = player.TPlayer.trashItem;
            //Item[] armor2 = player.TPlayer.Loadouts[0].Armor;
            //Item[] dye2 = player.TPlayer.Loadouts[0].Dye;
            //Item[] armor3 = player.TPlayer.Loadouts[1].Armor;
            //Item[] dye3 = player.TPlayer.Loadouts[1].Dye;
            //Item[] armor4 = player.TPlayer.Loadouts[2].Armor;
            //Item[] dye4 = player.TPlayer.Loadouts[2].Dye;
        }
        else
        {
            try
            {
                playerdata = TShock.CharacterDB.GetPlayerData(player, acc.ID);

                //改成for()循环
                for (int i = 0; i < playerdata.inventory.Length; i++)
                {
                    if (playerdata.inventory[i].NetId == item)
                    {
                        count += playerdata.inventory[i].Stack;
                        playerdata.inventory[i] = new(0, 0, 0);
                    }
                }

                //更新数据库背包
                TShock.CharacterDB.database.Query("UPDATE tsCharacter SET Inventory = @0 WHERE Account = @1", string.Join("~", playerdata.inventory), acc.ID);

            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return;
            }
        }
        args.Player.SendSuccessMessage($"已移除玩家{acc.Name}的{TShock.Utils.ItemTag(new() { netID = item, stack = 1, prefix = 0 })}X{count}");
        return;


    }

    private void RemoveItemChest(CommandArgs args)
    {
        if (args.Parameters.Count != 2)
        {
            args.Player.SendInfoMessage("用法:/rci <箱子ID> <物品名/ID>");
            return;
        }
        try
        {
            if (Main.chest[int.Parse(args.Parameters[0])] == null)
            {
                args.Player.SendErrorMessage($"找不到ID为{args.Parameters[0]}的箱子!");
                return;
            }

        }
        catch
        {
            args.Player.SendErrorMessage($"找不到ID为{args.Parameters[0]}的箱子!");
        }
        List<Item> itemByIdOrName = TShock.Utils.GetItemByIdOrName(args.Parameters[1]);
        if (itemByIdOrName.Count > 1)
        {
            args.Player.SendMultipleMatchError(from i in itemByIdOrName
                                               select (args.Player.RealPlayer ? string.Format("[i:{0}]", i.type) : "") + i.Name);
            return;
        }
        if (itemByIdOrName.Count == 0)
        {
            args.Player.SendErrorMessage("指定的物品无效");
            return;
        }
        int item = itemByIdOrName[0].netID;
        for (int i = 0; i < Main.chest[int.Parse(args.Parameters[0])].item.Length; i++)
        {
            if (Main.chest[int.Parse(args.Parameters[0])].item[i] != null && Main.chest[int.Parse(args.Parameters[0])].item[i].netID == item)
            {
                Main.chest[int.Parse(args.Parameters[0])].item[i] = new Item { netID = 0 };
            }
        }
        var chest = Main.chest[int.Parse(args.Parameters[0])];
        var itemStr = "";
        foreach (var i in chest.item)
        {
            if (i.netID == 0)
            {
                continue;
            }
            itemStr += TShock.Utils.ItemTag(i);
        }
        if (string.IsNullOrEmpty(itemStr))
        {
            itemStr = "空箱子";
        }
        args.Player.SendSuccessMessage($"箱子中的所有{TShock.Utils.ItemTag(new() { netID = item, stack = 1, prefix = 0 })}已被移除\n" +
            $"箱子ID:{args.Parameters[0]}\n" +
            $"坐标:({chest.x},{chest.y})\n" +
            $"名字:{(string.IsNullOrEmpty(chest.name) ? "无名箱子" : chest.name)}\n" +
            $"NPC商店:{(chest.bankChest ? "是" : "否")}\n" +
            $"物品:{itemStr}");

    }

    private void TpAllPlayer(CommandArgs args)
    {
        args.Player.SendInfoMessage("传送开始!");

        Task.Run(delegate
        {
            foreach (var i in TShock.Players)
            {
                if (i != null && args.Player.Active)
                    args.Player.Teleport(i.X, i.Y);
                Thread.Sleep(1000);
            }

        });

    }
    private void TpAllChest(CommandArgs args)
    {
        args.Player.SendInfoMessage("传送开始!");

        Task.Run(delegate
        {
            foreach (var i in Main.chest)
            {
                if (i != null && args.Player.Active)
                    args.Player.Teleport(i.x * 16, i.y * 16 + 2);
                Thread.Sleep(300);
            }

        });

    }

    private void InfoSearchCmd(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendInfoMessage("用法:/ci <箱子ID>");
            return;
        }
        try
        {
            if (Main.chest[int.Parse(args.Parameters[0])] == null)
            {
                args.Player.SendErrorMessage($"找不到ID为{args.Parameters[0]}的箱子!");
                return;
            }

        }
        catch
        {
            args.Player.SendErrorMessage($"找不到ID为{args.Parameters[0]}的箱子!");
        }
        var chest = Main.chest[int.Parse(args.Parameters[0])];
        var itemStr = "";
        foreach (var i in chest.item)
        {
            if (i.netID == 0)
            {
                continue;
            }
            itemStr += TShock.Utils.ItemTag(i);
        }
        if (string.IsNullOrEmpty(itemStr))
        {
            itemStr = "空箱子";
        }
        args.Player.SendSuccessMessage($"箱子的信息\n" +
            $"箱子ID:{args.Parameters[0]}\n" +
            $"坐标:({chest.x},{chest.y})\n" +
            $"名字:{(string.IsNullOrEmpty(chest.name) ? "无名箱子" : chest.name)}\n" +
            $"NPC商店:{(chest.bankChest ? "是" : "否")}\n" +
            $"物品:{itemStr}");
    }


    private void TpSearchCmd(CommandArgs args)
    {
        if (!args.Player.RealPlayer)
        {
            args.Player.SendErrorMessage("仅限游戏内使用");
            return;
        }
        if (args.Parameters.Count == 0)
        {
            args.Player.SendInfoMessage("用法:/tpc <箱子ID>");
            return;
        }
        try
        {
            if (Main.chest[int.Parse(args.Parameters[0])] == null)
            {
                args.Player.SendErrorMessage($"找不到ID为{args.Parameters[0]}的箱子!");
                return;
            }

        }
        catch
        {
            args.Player.SendErrorMessage($"找不到ID为{args.Parameters[0]}的箱子!");
        }

        args.Player.Teleport(Main.chest[int.Parse(args.Parameters[0])].x * 16, Main.chest[int.Parse(args.Parameters[0])].y * 16 + 2);
        args.Player.SendSuccessMessage($"已将你传送至箱子{args.Parameters[0]}");


    }

    private void ChestSearchCmd(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendInfoMessage("用法:/sc <物品ID/名称>");
            return;
        }
        List<Item> itemByIdOrName = TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
        if (itemByIdOrName.Count > 1)
        {
            args.Player.SendMultipleMatchError(from i in itemByIdOrName
                                               select (args.Player.RealPlayer ? string.Format("[i:{0}]", i.type) : "") + i.Name);
            return;
        }
        if (itemByIdOrName.Count == 0)
        {
            args.Player.SendErrorMessage("指定的物品无效");
            return;
        }
        int item = itemByIdOrName[0].type;
        List<(string, int)> list = new();
        for (int id = 0; id < Main.chest.Length; id++)
        {
            if (Main.chest[id] == null)
            {
                continue;
            }
            int items = 0;
            foreach (var c in Main.chest[id].item)
            {
                if (c.netID == item)
                {
                    items += c.stack;
                }

            }
            if (items != 0)
            {
                list.Add(($"[ID:{id}]({Main.chest[id].x},{Main.chest[id].y}) ", items));
            }
        }
        List<string> dataToPaginate = (from c in list
                                       orderby c.Item2 descending
                                       select string.Format("{0}{1}个", c.Item1, c.Item2)).ToList<string>();
        if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out var pageNumber))
        {
            args.Player.SendErrorMessage("无效的页码！");
            return;
        }
        if (dataToPaginate.Count > 0)
        {
            args.Player.SendSuccessMessage($"物品[i:{item}]在服务器箱子中的拥有情况:");
            args.Player.SendInfoMessage(string.Join("\n", dataToPaginate));
        }
        else
        {
            args.Player.SendWarningMessage($"当前服务器暂无箱子拥有[i:{item}]");
        }

    }

    private void ItemSearchCmd(CommandArgs args)
    {
        try
        {
            data.Clear();
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("用法:/si <物品ID/名称>\n其他命令:sc(查找箱子物品),ci(查询箱子信息),tpc(传送至指定箱子),tpallc(传送到地图的所有箱子),rci(移除箱子的物品),ri(移除玩家的指定物品)");
                return;
            }
            List<Item> itemByIdOrName = TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
            if (itemByIdOrName.Count > 1)
            {
                args.Player.SendMultipleMatchError(from i in itemByIdOrName
                                                   select (args.Player.RealPlayer ? string.Format("[i:{0}]", i.type) : "") + i.Name);
                return;
            }
            if (itemByIdOrName.Count == 0)
            {
                args.Player.SendErrorMessage("指定的物品无效");
                return;
            }
            int item = itemByIdOrName[0].type;
            TShock.Players.ForEach(delegate (TSPlayer p)
            {
                p?.SaveServerCharacter();
            });
            List<UserAccount> userAccounts = TShock.UserAccounts.GetUserAccounts();
            List<(string, int)> list = new List<(string, int)>();
            //queryResult = TShock.CharacterDB.database.QueryReader("SELECT * FROM tsCharacter");
            queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter");
            //queryResult.Read();
            while (queryResult.Reader.Read())
            {
                data.Add(queryResult.Reader.GetInt32(0), queryResult.Reader.GetString(5));
            }
            foreach (UserAccount item2 in userAccounts)
            {
                try
                {
                    if (TShock.Groups.GetGroupByName(item2.Group).HasPermission("tshock.ignore.bypassssc"))
                    {
                        continue;
                    }
                    List<NetItem> list2 = TryGetInventory(item2.ID);
                    if (list2 != null)
                    {
                        int num = list2.Where((NetItem i) => i.NetId == item).Sum((NetItem i) => i.Stack);
                        if (num > 0)
                        {
                            list.Add((item2.Name, num));
                        }
                    }
                    else
                    {
                        TShock.Log.Info("未能找到用户" + item2.Name + "的背包数据，已忽略");
                    }
                }
                catch
                {
                    TShock.Log.Info("用户" + item2.Name + "的背包错误，已忽略");
                }
            }
            List<string> dataToPaginate = (from c in list
                                           orderby c.Item2 descending
                                           select string.Format("[{0}]{1}个", c.Item1, c.Item2)).ToList<string>();
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out var pageNumber))
            {
                args.Player.SendErrorMessage("无效的页码！");
                return;
            }
            if (dataToPaginate.Count > 0)
            {
                args.Player.SendSuccessMessage($"物品[i:{item}]在服务器中的拥有情况:");
                args.Player.SendInfoMessage(string.Join("\n", dataToPaginate));
            }
            else
            {
                args.Player.SendWarningMessage($"当前服务器暂无玩家拥有[i:{item}]");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }

    private List<NetItem> TryGetInventory(int accid)
    {
        if (accid == -1)
        {
            return null;
        }
        try
        {
            data.TryGetValue(accid, out string inventory);
            List<NetItem> list = inventory.Split(new char[]
                    {
                        '~'
                    }).Select(new Func<string, NetItem>(NetItem.Parse)).ToList<NetItem>();
            if (list.Count < NetItem.MaxInventory)
            {
                list.InsertRange(67, new NetItem[2]);
                list.InsertRange(77, new NetItem[2]);
                list.InsertRange(87, new NetItem[2]);
                list.AddRange(new NetItem[NetItem.MaxInventory - list.Count]);
            }
            queryResult.Dispose();
            return list;
        }
        catch
        {
            return null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
        base.Dispose(disposing);
    }
}
