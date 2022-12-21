using Microsoft.Xna.Framework;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace ZHIPlayerManager
{
    public partial class ZHIPM : TerrariaPlugin
    {
        /// <summary>
        /// 帮助指令方法指令
        /// </summary>
        /// <param name="args"></param>
        private void Help(CommandArgs args)
        {
            if (args.Parameters.Count != 0)
            {
                args.Player.SendInfoMessage("输入 /zhelp  来查看指令帮助");
            }
            else
            {
                args.Player.SendInfoMessage("输入 /zsave    来备份自己的人物存档\n" +
                                            "输入 /zsaveauto [minute]    来每隔 minute 分钟自动备份自己的人物存档，当 minute 为 0 时关闭该功能\n" +
                                            "输入 /zvisa [num]    来查看自己的人物备份\n" +
                                            "输入 /zback [name]    来读取该玩家的人物存档\n" +
                                            "输入 /zback [name] [num]    来读取该玩家的第几个人物存档\n" +
                                            "输入 /zclone [name1] [name2]    将玩家1的人物数据复制给玩家2\n" +
                                            "输入 /zclone [name]    将该玩家的人物数据复制给自己\n" +
                                            "输入 /zmodify help    查看修改玩家数据的指令帮助\n" +
                                            "输入 /vi [name]    来查看该玩家的库存\n" +
                                            "输入 /vid [name]    来查看该玩家的库存，不分类\n" +
                                            "输入 /vit [name]    来查看该玩家的库存，不分类仅文本\n" +
                                            "输入 /vs [name]    来查看该玩家的状态\n" +
                                            "输入 /vst [name]    来查看该玩家的状态，仅文本\n" +
                                            "输入 /zfre [name]    来冻结该玩家\n" +
                                            "输入 /zunfre [name]    来解冻该玩家\n" +
                                            "输入 /zunfre all    来解冻所有玩家\n" +
                                            "输入 /zsort help    来查看排序系列指令帮助\n" +
                                            "输入 /zreset help    来查看zreset系列指令帮助\n" +
                                            "输入 /zban add [name]    来封禁无论是否在线的玩家");
            }
        }


        /// <summary>
        /// 回档指令方法指令
        /// </summary>
        /// <param name="args"></param>
        private void MySSCBack(CommandArgs args)
        {
            if (!TShock.ServerSideCharacterConfig.Settings.Enabled)
            {
                args.Player.SendInfoMessage("未启用SSC，回档功能不可用");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                MySSCBack2(args, 1);
                return;
            }
            if (args.Parameters.Count == 2)
            {
                if (!int.TryParse(args.Parameters[1], out int num))
                {
                    args.Player.SendInfoMessage("输入 /zback [name]  来读取该玩家的人物存档\n输入 /zback [name] [num]  来读取该玩家的第几个人物存档");
                    return;
                }
                if (num < 1 || num > 5)
                {
                    args.Player.SendInfoMessage("玩家最多有5个备份存档，范围 1 ~ 5，请重新输入");
                    return;
                }
                MySSCBack2(args, num);
            }
            else
            {
                args.Player.SendInfoMessage("输入 /zback [name]  来读取该玩家的人物存档\n输入 /zback [name] [num]  来读取该玩家的第几个人物存档");
            }
        }


        /// <summary>
        /// 保存指令方法指令
        /// </summary>
        /// <param name="args"></param>
        private void MySSCSave(CommandArgs args)
        {
            if (args.Parameters.Count != 0)
            {
                args.Player.SendInfoMessage("输入 /zsave  来备份自己的人物存档");
                return;
            }
            if (!TShock.ServerSideCharacterConfig.Settings.Enabled)
            {
                args.Player.SendInfoMessage("未启用SSC，备份功能不可用");
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendInfoMessage("对象不正确，请检查您的状态，您是否为游戏内玩家？");
                return;
            }
            if (ZPDataBase.AddZPlayerDB(args.Player))
            {
                ExtraData? extraData = edPlayers.Find((ExtraData x) => x.Name == args.Player.Name);
                if (extraData != null)
                {
                    ZPExtraDB.WriteExtraDB(extraData);
                }
                args.Player.SendMessage("您的备份保存成功！", new Color(0, 255, 0));
            }
            else
            {
                args.Player.SendMessage("您的备份保存失败！请尝试重进游戏重试", new Color(255, 0, 0));
            }
        }


        /// <summary>
        /// 自动备份指令
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MySSCSaveAuto(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zsaveauto [minute]  来每隔 minute 分钟自动备份自己的人物存档，当 minute 为 0 时关闭该功能");
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendInfoMessage("对象不正确，请检查您的状态，您是否为游戏内玩家？");
                return;
            }
            if (int.TryParse(args.Parameters[0], out int num))
            {
                if (num < 0)
                {
                    args.Player.SendInfoMessage("数字不合理");
                    return;
                }
                ExtraData? ex = edPlayers.Find(x => x.Name == args.Player.Name);
                if (ex == null)
                {
                    args.Player.SendInfoMessage("修改失败，请重进服务器重试");
                    return;
                }
                ex.backuptime = num;
                if (num != 0)
                    args.Player.SendMessage("修改成功，你的存档将每隔 " + num + " 分钟自动备份一次，请注意存档覆盖情况，这可能会覆盖你手动备份的部分", new Color(0, 255, 0));
                else
                    args.Player.SendMessage("修改成功，你的自动备份已关", new Color(0, 255, 0));
            }
            else
            {
                args.Player.SendInfoMessage("输入 /zsaveauto [minute]  来每隔 minute 分钟自动备份自己的人物存档，当 minute 为 0 时关闭该功能");
            }
        }


        /// <summary>
        /// 查看我的存档方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ViewMySSCSave(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendInfoMessage("输入 /zvisa [num] 来查看自己的人物备份");
                return;
            }
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out int num))
            {
                args.Player.SendInfoMessage("输入 /zvisa [num] 来查看自己的人物备份");
                return;
            }
            if (args.Parameters.Count == 1 && int.TryParse(args.Parameters[0], out int num2) && (num2 < 1 || num2 > 5))
            {
                args.Player.SendInfoMessage("玩家最多有5个备份存档，范围 1 ~ 5，请重新输入");
            }
            else
            {
                if (args.Player == null || !args.Player.IsLoggedIn)
                {
                    args.Player.SendInfoMessage("对象不正确，请检查您的状态，您是否为游戏内玩家？");
                    return;
                }
                int slot;
                if (args.Parameters.Count == 0)
                {
                    slot = 1;
                }
                else
                {
                    slot = int.Parse(args.Parameters[0]);
                }
                PlayerData playerData = ZPDataBase.ReadZPlayerDB(args.Player, args.Player.Account.ID, slot);
                if (playerData == null || !playerData.exists)
                {
                    args.Player.SendInfoMessage("您还未备份");
                }
                else
                {
                    Item[] items = new Item[NetItem.MaxInventory];
                    for (int i = 0; i < NetItem.MaxInventory; i++)
                    {
                        items[i] = TShock.Utils.GetItemById(playerData.inventory[i].NetId);
                        items[i].stack = playerData.inventory[i].Stack;
                        items[i].prefix = playerData.inventory[i].PrefixId;
                    }
                    string text = GetItemsString(items, NetItem.MaxInventory, 0);
                    text = FormatArrangement(text, 30, " ");
                    string str = "您的备份 [ " + args.Player.Account.ID + " - " + slot + " ] 的内容为：\n" + text;
                    args.Player.SendInfoMessage(str);
                }
            }
        }


        /// <summary>
        /// 克隆另一个人的数据的方法指令
        /// </summary>
        /// <param name="args"></param>
        private void SSCClone(CommandArgs args)
        {
            if (args.Parameters.Count == 0 || args.Parameters.Count > 2)
            {
                args.Player.SendInfoMessage("输入 /zclone [name1] [name2]  将玩家1的人物数据复制给玩家2\n输入 /zclone [name]  将该玩家的人物数据复制给自己");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0] == args.Player.Name)
                {
                    args.Player.SendMessage("克隆失败！请不要克隆自己", new Color(255, 0, 0));
                    return;
                }
                if (!args.Player.IsLoggedIn)
                {
                    args.Player.SendInfoMessage("对象不正确，请检查您的状态，您是否为游戏内玩家？");
                    return;
                }
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                //找不到人，查离线
                if (list.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (user == null)
                    {
                        if (users.Count == 0)
                        {
                            args.Player.SendInfoMessage(noplayer);
                            return;
                        }
                        else if (users.Count > 1)
                        {
                            args.Player.SendInfoMessage(manyplayer);
                            return;
                        }
                        else
                            user = users[0];
                    }
                    PlayerData playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user.ID);
                    if (UpdatePlayerAll(args.Player, playerData))
                    {
                        args.Player.SendMessage("克隆成功！您已将玩家 [" + user.Name + "] 的数据克隆到你身上", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("克隆失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                }
                //人太多，舍弃
                else if (list.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                //一个在线
                else
                {
                    PlayerData playerData = list[0].PlayerData;
                    playerData.CopyCharacter(list[0]);
                    playerData.exists = true;
                    if (UpdatePlayerAll(args.Player, playerData))
                    {
                        args.Player.SendMessage("克隆成功！您已将玩家 [" + list[0].Name + "] 的数据克隆到你身上", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("克隆失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                }
            }
            if (args.Parameters.Count == 2)
            {
                List<TSPlayer> player1 = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                List<TSPlayer> player2 = BestFindPlayerByNameOrIndex(args.Parameters[1]);
                if (player1.Count > 1 || player2.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                //都在线的情况
                if (player1.Count == 1 && player2.Count == 1)
                {
                    if (player1[0].Name == player2[0].Name)
                    {
                        args.Player.SendInfoMessage("请不要对同一个人进行克隆");
                        return;
                    }
                    player1[0].PlayerData.CopyCharacter(player1[0]);
                    player1[0].PlayerData.exists = true;
                    if (UpdatePlayerAll(player2[0], player1[0].PlayerData))
                    {
                        if (args.Player.Account.ID != player2[0].Account.ID)
                        {
                            args.Player.SendMessage($"克隆成功！您已将玩家 [{player1[0].Name}] 的数据克隆到 [{player2[0].Name}] 身上", new Color(0, 255, 0));
                        }
                        else
                        {
                            player2[0].SendMessage("克隆成功！已将玩家 [" + player1[0].Name + "] 的数据克隆到你身上", new Color(0, 255, 0));
                        }
                    }
                    else
                    {
                        args.Player.SendMessage("克隆失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                    return;
                }
                //赋值者不在线，被赋值者在线的情况
                if (player1.Count == 0 && player2.Count == 1)
                {
                    args.Player.SendInfoMessage("玩家1不在线，正在查询离线数据");
                    UserAccount user1 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    List<UserAccount> user1s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (user1 == null)
                    {
                        if (user1s.Count == 0)
                        {
                            args.Player.SendInfoMessage("玩家1不存在");
                            return;
                        }
                        else if (user1s.Count > 1)
                        {
                            args.Player.SendInfoMessage("玩家1不唯一");
                            return;
                        }
                        else
                            user1 = user1s[0];
                    }
                    PlayerData playerData1 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user1.ID);
                    if (UpdatePlayerAll(player2[0], playerData1))
                    {
                        if (args.Player.Account.ID != player2[0].Account.ID)
                        {
                            args.Player.SendMessage($"克隆成功！您已将玩家 [{user1.Name}] 的数据克隆到玩家 [{player2[0].Name}]身上", new Color(0, 255, 0));
                        }
                        else
                        {
                            player2[0].SendMessage("克隆成功！已将玩家 [" + user1.Name + "] 的数据克隆到你身上", new Color(0, 255, 0));
                        }
                    }
                    else
                    {
                        args.Player.SendMessage("克隆失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                    return;
                }
                //赋值者在线，被赋值者不在线的情况
                if (player1.Count == 1 && player2.Count == 0)
                {
                    args.Player.SendInfoMessage("玩家2不在线，正在查询离线数据");
                    UserAccount user2 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                    List<UserAccount> user2s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[1], true);
                    if (user2 == null)
                    {
                        if (user2s.Count == 0)
                        {
                            args.Player.SendInfoMessage("玩家2不存在");
                            return;
                        }
                        else if (user2s.Count > 1)
                        {
                            args.Player.SendInfoMessage("玩家2不唯一");
                            return;
                        }
                        else
                            user2 = user2s[0];
                    }
                    PlayerData playerData1 = player1[0].PlayerData;
                    playerData1.exists = true;
                    if (UpdateTshockDBCharac(user2.ID, playerData1))
                    {
                        args.Player.SendMessage($"克隆成功！您已将玩家 [{player1[0].Name}] 的数据克隆到玩家 [{user2.Name}] 身上", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("克隆失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                    return;
                }
                //都不在线
                if (player1.Count == 0 && player2.Count == 0)
                {
                    args.Player.SendInfoMessage("玩家都不在线，正在查询离线数据");
                    UserAccount user1 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    List<UserAccount> user1s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    UserAccount user2 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                    List<UserAccount> user2s = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[1], true);
                    if (user1 == null)
                    {
                        if (user1s.Count == 0)
                        {
                            args.Player.SendInfoMessage("玩家1不存在");
                            return;
                        }
                        else if (user1s.Count > 1)
                        {
                            args.Player.SendInfoMessage("玩家1不唯一");
                            return;
                        }
                        else
                            user1 = user1s[0];
                    }
                    if (user2 == null)
                    {
                        if (user2s.Count == 0)
                        {
                            args.Player.SendInfoMessage("玩家2不存在");
                            return;
                        }
                        else if (user2s.Count > 1)
                        {
                            args.Player.SendInfoMessage("玩家2不唯一");
                            return;
                        }
                        else
                            user2 = user2s[0];
                    }
                    PlayerData playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user1.ID);
                    if (UpdateTshockDBCharac(user2.ID, playerData))
                    {
                        args.Player.SendMessage($"克隆成功！您已将玩家 [{user1.Name}] 的数据克隆到玩家 [{user2.Name}] 身上", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("克隆失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                    return;
                }
            }
        }


        /// <summary>
        /// 修改人物数据方法指令
        /// </summary>
        /// <param name="args"></param>
        private void SSCModify(CommandArgs args)
        {
            if (args.Parameters.Count != 1 && args.Parameters.Count != 3)
            {
                args.Player.SendInfoMessage("输入 /zmodify help  查看修改玩家数据的指令帮助");
                return;
            }
            if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    args.Player.SendInfoMessage(
                        "输入 /zmodify [name] life [num]  来修改玩家的血量\n" +
                        "输入 /zmodify [name] lifemax [num]  来修改玩家的血量上限\n" +
                        "输入 /zmodify [name] mana [num]  来修改玩家的魔力\n" +
                        "输入 /zmodify [name] manamax [num]  来修改玩家的魔力上限\n" +
                        "输入 /zmodify [name] fish [num]  来修改玩家的渔夫任务数\n" +
                        "输入 /zmodify [name] torch [0或1]  来关闭或开启火把神增益\n" +
                        "输入 /zmodify [name] demmon [0或1]  来关闭或开启恶魔心增益\n" +
                        "输入 /zmodify [name] bread [0或1]  来关闭或开启工匠面包增益\n" +
                        "输入 /zmodify [name] heart [0或1]  来关闭或开启埃癸斯水晶增益\n" +
                        "输入 /zmodify [name] fruit [0或1]  来关闭或开启埃癸斯果增益\n" +
                        "输入 /zmodify [name] star [0或1]  来关闭或开启奥术水晶增益\n" +
                        "输入 /zmodify [name] pearl [0或1]  来关闭或开启银河珍珠增益\n" +
                        "输入 /zmodify [name] worm [0或1]  来关闭或开启粘性蠕虫增益\n" +
                        "输入 /zmodify [name] ambrosia [0或1]  来关闭或开启珍馐增益\n" +
                        "输入 /zmodify [name] cart [0或1]  来关闭或开启超级矿车增益\n" +
                        "输入 /zmodify [name] enhance [0或1]  来关闭或开启所有玩家增益"
                        );
                }
                else
                {
                    args.Player.SendInfoMessage("输入 /zmodify help  查看修改玩家数据的指令帮助");
                }
                return;
            }
            if (args.Parameters.Count == 3)
            {
                //对参数3先判断是不是数据，不是数字结束
                if (!int.TryParse(args.Parameters[2], out int num))
                {
                    args.Player.SendInfoMessage("格式错误！输入 /zmodify help  查看修改玩家数据的指令帮助");
                    return;
                }
                //再判断能不能找到人的情况
                List<TSPlayer> players = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                if (players.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                if (players.Count == 1)
                {
                    if (args.Parameters[1].Equals("life", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statLife = num;
                        players[0].SendData(PacketTypes.PlayerHp, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的生命值已被修改为：" + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("lifemax", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statLifeMax = num;
                        players[0].SendData(PacketTypes.PlayerHp, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的生命上限已被修改为：" + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("mana", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statMana = num;
                        players[0].SendData(PacketTypes.PlayerMana, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的魔力值已被修改为：" + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("manamax", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.statManaMax = num;
                        players[0].SendData(PacketTypes.PlayerMana, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的魔力上限已被修改为：" + num, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("fish", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.anglerQuestsFinished = num;
                        players[0].SendData(PacketTypes.NumberOfAnglerQuestsCompleted, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的渔夫任务完成数已被修改为：" + num, new Color(0, 255, 0));
                    }
                    else if (num != 0 && num != 1)
                    {
                        args.Player.SendInfoMessage("格式错误！输入 /zmodify help  查看修改玩家数据的指令帮助");
                        return;
                    }
                    else if (args.Parameters[1].Equals("torch", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.unlockedBiomeTorches = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的火把神增益开启状态：" + players[0].TPlayer.unlockedBiomeTorches, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("demmon", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.extraAccessory = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的恶魔心增益开启状态：" + players[0].TPlayer.extraAccessory, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("bread", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.ateArtisanBread = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的工匠面包增益开启状态：" + players[0].TPlayer.ateArtisanBread, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("heart", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedAegisCrystal = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的埃癸斯水晶增益开启状态：" + players[0].TPlayer.usedAegisCrystal, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("fruit", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedAegisFruit = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的埃癸斯果增益开启状态：" + players[0].TPlayer.usedAegisFruit, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("star", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedArcaneCrystal = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的奥术水晶增益开启状态：" + players[0].TPlayer.usedArcaneCrystal, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("pearl", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedGalaxyPearl = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的银河珍珠增益开启状态：" + players[0].TPlayer.usedGalaxyPearl, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("worm", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedGummyWorm = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的粘性蠕虫增益开启状态：" + players[0].TPlayer.usedGummyWorm, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("ambrosia", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.usedAmbrosia = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的珍馐增益开启状态：" + players[0].TPlayer.usedAmbrosia, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("cart", StringComparison.OrdinalIgnoreCase))
                    {
                        players[0].TPlayer.unlockedSuperCart = (num != 0);
                        players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                        players[0].SendMessage("您的超级矿车增益开启状态：" + players[0].TPlayer.unlockedSuperCart, new Color(0, 255, 0));
                    }
                    else if (args.Parameters[1].Equals("enhance", StringComparison.OrdinalIgnoreCase))
                    {
                        if (num == 1)
                        {
                            players[0].TPlayer.unlockedBiomeTorches = true;
                            players[0].TPlayer.extraAccessory = true;
                            players[0].TPlayer.ateArtisanBread = true;
                            players[0].TPlayer.usedAegisCrystal = true;
                            players[0].TPlayer.usedAegisFruit = true;
                            players[0].TPlayer.usedArcaneCrystal = true;
                            players[0].TPlayer.usedGalaxyPearl = true;
                            players[0].TPlayer.usedGummyWorm = true;
                            players[0].TPlayer.usedAmbrosia = true;
                            players[0].TPlayer.unlockedSuperCart = true;
                            players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                            players[0].SendMessage("您的所有永久增益均开启", new Color(0, 255, 0));
                        }
                        else if (num == 0)
                        {
                            players[0].TPlayer.unlockedBiomeTorches = false;
                            players[0].TPlayer.extraAccessory = false;
                            players[0].TPlayer.ateArtisanBread = false;
                            players[0].TPlayer.usedAegisCrystal = false;
                            players[0].TPlayer.usedAegisFruit = false;
                            players[0].TPlayer.usedArcaneCrystal = false;
                            players[0].TPlayer.usedGalaxyPearl = false;
                            players[0].TPlayer.usedGummyWorm = false;
                            players[0].TPlayer.usedAmbrosia = false;
                            players[0].TPlayer.unlockedSuperCart = false;
                            players[0].SendData(PacketTypes.PlayerInfo, "", players[0].Index, 0f, 0f, 0f, 0);
                            players[0].SendMessage("您的所有永久增益均关闭", new Color(0, 255, 0));
                        }
                        else
                        {
                            args.Player.SendInfoMessage("格式错误！输入 /zmodify help  查看修改玩家数据的指令帮助");
                        }
                    }
                    args.Player.SendMessage("修改成功！", new Color(0, 255, 0));
                }
                else if (players.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    if (user == null)
                    {
                        List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                        if (users.Count == 0)
                        {
                            args.Player.SendInfoMessage(noplayer);
                            return;
                        }
                        else if (users.Count > 1)
                        {
                            args.Player.SendInfoMessage(manyplayer);
                            return;
                        }
                        else
                            user = users[0];

                    }
                    try
                    {
                        if (args.Parameters[1].Equals("life", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET Health = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("lifemax", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET MaxHealth= @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("mana", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET Mana = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("manamax", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET MaxMana = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("fish", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET questsCompleted = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (num != 0 && num != 1)
                        {
                            args.Player.SendInfoMessage("格式错误！输入 /zmodify help  查看修改玩家数据的指令帮助");
                            return;
                        }
                        else if (args.Parameters[1].Equals("torch", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET unlockedBiomeTorches = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("demmon", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET extraSlot = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("bread", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET ateArtisanBread = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("crystal", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedAegisCrystal = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("fruit", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedAegisFruit = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("arcane", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedArcaneCrystal = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("pearl", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedGalaxyPearl = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("worm", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedGummyWorm = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("ambrosia", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET usedAmbrosia = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("cart", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET unlockedSuperCart = @0 WHERE Account = @1;", new object[]
                            {
                                    num,
                                    user.ID
                            });
                        }
                        else if (args.Parameters[1].Equals("enhance", StringComparison.OrdinalIgnoreCase))
                        {
                            TShock.DB.Query("UPDATE tsCharacter SET unlockedBiomeTorches = @1, extraSlot = @2, ateArtisanBread = @3, usedAegisCrystal = @4, usedAegisFruit = @5, usedArcaneCrystal = @6, usedGalaxyPearl = @7, usedGummyWorm = @8, usedAmbrosia = @9, unlockedSuperCart = @10 WHERE Account = @0;", new object[]
                            {
                                    user.ID, num, num, num, num, num, num, num, num, num, num
                            });
                        }
                        args.Player.SendMessage("修改成功！", new Color(0, 255, 0));
                    }
                    catch (Exception ex)
                    {
                        args.Player.SendMessage("修改失败！错误：" + ex.ToString(), new Color(255, 0, 0));
                        TShock.Log.Error("修改失败！错误：" + ex.ToString());
                    }
                }
            }
        }


        /// <summary>
        /// 清理该玩家身上的所有buff
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ZResetPlayerBuff(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zresetbuff [name]  来清理该玩家的所有Buff\n输入 /zresetbuff all  来清理所有玩家所有Buff");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                foreach (TSPlayer tSPlayer in TShock.Players)
                {
                    if (tSPlayer != null && tSPlayer.IsLoggedIn)
                    {
                        clearAllBuffFromPlayer(tSPlayer);
                    }
                }
                args.Player.SendMessage($"所有玩家的所有Buff均已消除", new Color(0, 255, 0));
                return;
            }
            List<TSPlayer> ts = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (ts.Count == 0)
            {
                args.Player.SendInfoMessage("该玩家不在线或不存在");
            }
            else if (ts.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else
            {
                clearAllBuffFromPlayer(ts[0]);
                args.Player.SendMessage($"玩家 [ {ts[0].Name} ] 的所有Buff均已消除", new Color(0, 255, 0));
            }
        }


        /// <summary>
        /// 重置用户备份数据库方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayerDB(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zresetdb [name]  来清理该玩家的备份数据\n输入 /zresetdb all  来清理所有玩家的备份数据");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (ZPDataBase.ClearALLZPlayerDB(ZPDataBase))
                {
                    if (!args.Player.IsLoggedIn)
                    {
                        args.Player.SendMessage("所有玩家的备份数据均已重置", broadcastColor);
                        TSPlayer.All.SendMessage("所有玩家的备份数据均已重置", broadcastColor);
                    }
                    else
                    {
                        TSPlayer.All.SendMessage("所有玩家的备份数据均已重置", broadcastColor);
                    }
                }
                else
                {
                    args.Player.SendMessage("重置失败", new Color(255, 0, 0));
                }
            }
            else
            {
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
                if (list.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                if (list.Count == 1)
                {
                    if (ZPDataBase.ClearZPlayerDB(list[0].Account.ID))
                    {
                        args.Player.SendMessage("重置成功", new Color(0, 255, 0));
                        list[0].SendMessage("您的备份数据已重置", broadcastColor);
                    }
                    else
                    {
                        args.Player.SendMessage("重置失败", new Color(255, 0, 0));
                    }
                    return;
                }
                if (list.Count == 0)
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                    if (user == null)
                    {
                        args.Player.SendMessage(noplayer, new Color(255, 0, 0));
                    }
                    else
                    {
                        if (ZPDataBase.ClearZPlayerDB(user.ID))
                        {
                            args.Player.SendMessage("重置成功", new Color(0, 255, 0));
                        }
                        else
                        {
                            args.Player.SendMessage("重置失败", new Color(255, 0, 0));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 重置用户额外数据库方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayerEX(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zresetex [name]  来清理该玩家的额外数据\n输入 /zresetex all  来清理所有玩家的额外数据");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (ZPExtraDB.ClearALLZPlayerExtraDB(ZPExtraDB))
                {
                    edPlayers.Clear();
                    if (!args.Player.IsLoggedIn)
                    {
                        args.Player.SendMessage("所有玩家的额外数据均已重置", broadcastColor);
                        TSPlayer.All.SendMessage("所有玩家的额外数据均已重置", broadcastColor);
                    }
                    else
                    {
                        TSPlayer.All.SendMessage("所有玩家的额外数据均已重置", broadcastColor);
                    }
                }
                else
                {
                    args.Player.SendMessage("重置失败", new Color(255, 0, 0));
                }
                return;
            }
            List<TSPlayer> tSPlayers = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (tSPlayers.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
                return;
            }
            if (tSPlayers.Count == 1)
            {
                if (ZPExtraDB.ClearZPlayerExtraDB(tSPlayers[0].Account.ID))
                {
                    edPlayers.RemoveAll((ExtraData x) => x.Name == tSPlayers[0].Name);
                    args.Player.SendMessage("重置成功", new Color(0, 255, 0));
                    tSPlayers[0].SendMessage("您的额外数据已重置", broadcastColor);
                }
                else
                {
                    args.Player.SendMessage("重置失败", new Color(255, 0, 0));
                }
                return;
            }
            if (tSPlayers.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user == null)
                {
                    args.Player.SendMessage(noplayer, new Color(255, 0, 0));
                }
                else
                {
                    if (ZPExtraDB.ClearZPlayerExtraDB(user.ID))
                    {
                        args.Player.SendMessage("重置成功", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("重置失败", new Color(255, 0, 0));
                    }
                }
            }
        }


        /// <summary>
        /// 重置玩家的人物数据方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zreset [name]  来清理该玩家的人物数据\n输入 /zreset all  来清理所有玩家的人物数据");
                return;
            }
            if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                args.Player.SendInfoMessage(
                                            "输入 /zresetbuff [name]  来清理该玩家的所有Buff\n" +
                                            "输入 /zresetbuff all  来清理所有玩家所有Buff\n" +
                                            "输入 /zresetdb [name]  来清理该玩家的备份数据\n" +
                                            "输入 /zresetdb all  来清理所有玩家的备份数据\n" +
                                            "输入 /zresetex [name]  来清理该玩家的额外数据\n" +
                                            "输入 /zresetex all  来清理所有玩家的额外数据\n" +
                                            "输入 /zreset [name]  来清理该玩家的人物数据\n" +
                                            "输入 /zreset all  来清理所有玩家的人物数据\n" +
                                            "输入 /zresetallplayers  来清理所有玩家的所有数据"
                                            );
                return;
            }
            else if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                foreach (TSPlayer ts in TShock.Players)
                {
                    if (ts != null && ts.IsLoggedIn)
                    {
                        ResetPlayer(ts);
                    }
                }
                TShock.DB.Query("delete from tsCharacter");
                if (!args.Player.IsLoggedIn)
                {
                    args.Player.SendMessage("所有玩家的人物数据均已重置", broadcastColor);
                    TSPlayer.All.SendMessage("所有玩家的人物数据均已重置", broadcastColor);
                }
                else
                {
                    TSPlayer.All.SendMessage("所有玩家的人物数据均已重置", broadcastColor);
                }
                return;
            }
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (list.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
                return;
            }
            if (list.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user == null)
                {
                    args.Player.SendInfoMessage(noplayer);
                }
                else
                {
                    if (TShock.CharacterDB.RemovePlayer(user.ID))
                    {
                        args.Player.SendMessage("已重置离线玩家数据", new Color(0, 255, 0));
                    }
                    else
                    {
                        args.Player.SendMessage("重置失败！未在原数据库中查到该玩家，请检查输入是否正确，该玩家是否避免SSC检测，再重新输入", new Color(255, 0, 0));
                    }
                }
                return;
            }
            if (list.Count == 1)
            {
                if (ResetPlayer(list[0]) | TShock.CharacterDB.RemovePlayer(list[0].Account.ID))
                {
                    args.Player.SendMessage("已重置该玩家数据", new Color(0, 255, 0));
                    list[0].SendMessage("您的人物数据已被重置", broadcastColor);
                }
                else
                {
                    args.Player.SendInfoMessage("重置失败");
                }
                return;
            }
        }


        /// <summary>
        /// 重置所有用户所有数据方法指令
        /// </summary>
        /// <param name="args"></param>
        private void ZResetPlayerAll(CommandArgs args)
        {
            if (args.Parameters.Count != 0)
            {
                args.Player.SendInfoMessage("输入 / zresetallplayers  来清理所有玩家的所有数据");
                return;
            }
            try
            {
                foreach (TSPlayer tsplayer in TShock.Players)
                {
                    if (tsplayer != null && tsplayer.IsLoggedIn)
                    {
                        ResetPlayer(tsplayer);
                    }
                }
                TShock.DB.Query("delete from tsCharacter");
                ZPDataBase.ClearALLZPlayerDB(ZPDataBase);
                ZPExtraDB.ClearALLZPlayerExtraDB(ZPExtraDB);
                edPlayers.Clear();
            }
            catch (Exception ex)
            {
                args.Player.SendMessage("清理失败 ZResetPlayerAll :" + ex.ToString(), new Color(255, 0, 0));
                TShock.Log.Error("清理失败 ZResetPlayerAll :" + ex.ToString());
                return;
            }
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendMessage("玩家已全部初始化", new Color(0, 255, 0));
                TSPlayer.All.SendMessage("所有玩家的所有数据均已全部初始化", broadcastColor);
            }
            else
            {
                TShock.Utils.Broadcast("所有玩家的所有数据均已全部初始化", broadcastColor);
            }
        }


        /// <summary>
        /// 分类查阅指令
        /// </summary>
        /// <param name="args"></param>
        private void ViewInvent(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /vi [玩家名]  来查看该玩家的库存");
                return;
            }
            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (list.Any())
            {
                foreach (var li in list)
                {
                    StringBuilder sb = new StringBuilder();
                    string inventory = GetItemsString(li.TPlayer.inventory, NetItem.InventorySlots);

                    //装备栏一堆
                    string armor = GetItemsString(li.TPlayer.armor, NetItem.ArmorSlots);
                    string armor1 = GetItemsString(li.TPlayer.Loadouts[0].Armor, NetItem.ArmorSlots);
                    string armor2 = GetItemsString(li.TPlayer.Loadouts[1].Armor, NetItem.ArmorSlots);
                    string armor3 = GetItemsString(li.TPlayer.Loadouts[2].Armor, NetItem.ArmorSlots);

                    //染料一堆
                    string dyestuff = GetItemsString(li.TPlayer.dye, NetItem.DyeSlots);
                    string dyestuff1 = GetItemsString(li.TPlayer.Loadouts[0].Dye, NetItem.DyeSlots);
                    string dyestuff2 = GetItemsString(li.TPlayer.Loadouts[1].Dye, NetItem.DyeSlots);
                    string dyestuff3 = GetItemsString(li.TPlayer.Loadouts[2].Dye, NetItem.DyeSlots);

                    string misc = GetItemsString(li.TPlayer.miscEquips, NetItem.MiscEquipSlots);
                    string miscDye = GetItemsString(li.TPlayer.miscDyes, NetItem.MiscDyeSlots);
                    string trash = string.Format("【[i/s{0}:{1}]】 ", li.TPlayer.trashItem.stack, li.TPlayer.trashItem.netID);

                    string pig = GetItemsString(li.TPlayer.bank.item, NetItem.PiggySlots);
                    string safe = GetItemsString(li.TPlayer.bank2.item, NetItem.SafeSlots);
                    string forge = GetItemsString(li.TPlayer.bank3.item, NetItem.ForgeSlots);
                    string vault = GetItemsString(li.TPlayer.bank4.item, NetItem.VoidSlots);

                    if (list.Count == 1)
                        sb.AppendLine("玩家 【" + li.Name + "】 的所有库存如下:");
                    else
                        sb.AppendLine("多个结果  玩家 【" + li.Name + "】 的所有库存如下:");

                    if (inventory.Length > 0 && inventory != null && inventory != "")
                    {
                        sb.AppendLine("背包:");
                        sb.AppendLine(FormatArrangement(inventory, 30, " "));
                    }
                    //装备栏
                    if (armor.Length > 0 && armor != null && armor != "")
                    {
                        sb.AppendLine("盔甲 + 饰品 + 时装:");
                        sb.AppendLine("当前装备栏：");
                        sb.AppendLine(armor);
                    }
                    if (armor1.Length > 0 && armor1 != null && armor1 != "")
                    {
                        sb.AppendLine("装备栏1：");
                        sb.AppendLine(armor1);
                    }
                    if (armor2.Length > 0 && armor2 != null && armor2 != "")
                    {
                        sb.AppendLine("装备栏2：");
                        sb.AppendLine(armor2);
                    }
                    if (armor3.Length > 0 && armor3 != null && armor3 != "")
                    {
                        sb.AppendLine("装备栏3：");
                        sb.AppendLine(armor3);
                    }
                    //染料
                    if (dyestuff.Length > 0 && dyestuff != null && dyestuff != "")
                    {
                        sb.AppendLine("当前染料:");
                        sb.AppendLine(dyestuff);
                    }
                    if (dyestuff1.Length > 0 && dyestuff1 != null && dyestuff1 != "")
                    {
                        sb.AppendLine("染料1:");
                        sb.AppendLine(dyestuff1);
                    }
                    if (dyestuff2.Length > 0 && dyestuff2 != null && dyestuff2 != "")
                    {
                        sb.AppendLine("染料2:");
                        sb.AppendLine(dyestuff2);
                    }
                    if (dyestuff3.Length > 0 && dyestuff3 != null && dyestuff3 != "")
                    {
                        sb.AppendLine("染料3:");
                        sb.AppendLine(dyestuff3);
                    }


                    if (misc.Length > 0 && misc != null && misc != "")
                    {
                        sb.AppendLine("宠物 + 矿车 + 坐骑 + 钩爪:");
                        sb.AppendLine(misc);
                    }
                    if (miscDye.Length > 0 && miscDye != null && miscDye != "")
                    {
                        sb.AppendLine("宠物 矿车 坐骑 钩爪 染料:");
                        sb.AppendLine(miscDye);
                    }
                    if (trash != "【[i/s0:0]】 ")
                    {
                        sb.AppendLine("垃圾桶:");
                        sb.AppendLine(trash);
                    }
                    if (pig.Length > 0 && pig != null && pig != "")
                    {
                        sb.AppendLine("猪猪储蓄罐:");
                        sb.AppendLine(FormatArrangement(pig, 30, " "));
                    }
                    if (safe.Length > 0 && safe != null && safe != "")
                    {
                        sb.AppendLine("保险箱:");
                        sb.AppendLine(FormatArrangement(safe, 30, " "));
                    }
                    if (forge.Length > 0 && forge != null && forge != "")
                    {
                        sb.AppendLine("护卫熔炉:");
                        sb.AppendLine(FormatArrangement(forge, 30, " "));
                    }
                    if (vault.Length > 0 && vault != null && vault != "")
                    {
                        sb.AppendLine("虚空金库:");
                        sb.AppendLine(FormatArrangement(vault, 30, " "));
                    }
                    if (sb.Length > 0 && sb != null && sb.ToString() != "")
                        args.Player.SendMessage(sb.ToString() + "\n", TextColor());
                    else
                        args.Player.SendInfoMessage("玩家 【" + li.Name + "】 未携带任何东西");
                }
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                foreach (var t in temp)
                {
                    PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                    if (t != null && t2.exists)
                    {
                        users.Add(t, t2);
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendErrorMessage("未查找到该玩家！");
                }
                else if (users.Count == 1)
                {
                    string offAll = GetItemsString(users.First().Value.inventory, users.First().Value.inventory.Length);
                    offAll = FormatArrangement(offAll, 30, " ");
                    if (!string.IsNullOrEmpty(offAll))
                    {
                        args.Player.SendMessage("玩家 【" + users.First().Key.Name + "】 的所有库存如下:" + "\n" + offAll, TextColor());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("玩家 【" + users.First().Key.Name + "】 未携带任何东西");
                    }
                }
                else
                {
                    foreach (var p in users)
                    {
                        string offAll = GetItemsString(p.Value.inventory, p.Value.inventory.Length);
                        offAll = FormatArrangement(offAll, 30, " ");
                        offAll += "\n";
                        if (!string.IsNullOrEmpty(offAll))
                        {
                            args.Player.SendMessage("多个结果  玩家 【" + p.Key.Name + "】 的所有库存如下:" + "\n" + offAll, TextColor());
                        }
                        else
                        {
                            args.Player.SendInfoMessage("玩家 【" + p.Key.Name + "】 未携带任何东西\n");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 不分类查阅指令
        /// </summary>
        /// <param name="args"></param>
        private void ViewInventDisorder(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /vid [玩家名]  来查看该玩家的库存，不进行排列");
                return;
            }
            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (list.Any())
            {
                foreach (var li in list)
                {
                    string inventory = GetItemsString(li.TPlayer.inventory, NetItem.InventorySlots);

                    string armor = GetItemsString(li.TPlayer.armor, NetItem.ArmorSlots);
                    string armor1 = GetItemsString(li.TPlayer.Loadouts[0].Armor, NetItem.ArmorSlots);
                    string armor2 = GetItemsString(li.TPlayer.Loadouts[1].Armor, NetItem.ArmorSlots);
                    string armor3 = GetItemsString(li.TPlayer.Loadouts[2].Armor, NetItem.ArmorSlots);


                    string dyestuff = GetItemsString(li.TPlayer.dye, NetItem.DyeSlots);
                    string dyestuff1 = GetItemsString(li.TPlayer.Loadouts[0].Dye, NetItem.DyeSlots);
                    string dyestuff2 = GetItemsString(li.TPlayer.Loadouts[1].Dye, NetItem.DyeSlots);
                    string dyestuff3 = GetItemsString(li.TPlayer.Loadouts[2].Dye, NetItem.DyeSlots);


                    string misc = GetItemsString(li.TPlayer.miscEquips, NetItem.MiscEquipSlots);
                    string miscDye = GetItemsString(li.TPlayer.miscDyes, NetItem.MiscDyeSlots);
                    string trash = string.Format("【[i/s{0}:{1}]】 ", li.TPlayer.trashItem.stack, li.TPlayer.trashItem.netID);

                    string pig = GetItemsString(li.TPlayer.bank.item, NetItem.PiggySlots);
                    string safe = GetItemsString(li.TPlayer.bank2.item, NetItem.SafeSlots);
                    string forge = GetItemsString(li.TPlayer.bank3.item, NetItem.ForgeSlots);
                    string vault = GetItemsString(li.TPlayer.bank4.item, NetItem.VoidSlots);

                    if (trash == "【[i/s0:0]】 ")
                        trash = "";

                    string all = inventory + armor + armor1 + armor2 + armor3 + dyestuff + dyestuff1 + dyestuff2 + dyestuff3 + misc + misc + miscDye + trash + pig + safe + forge + vault;
                    all = FormatArrangement(all, 30, " ");
                    if (all != "")
                    {
                        if (list.Count == 1)
                            args.Player.SendMessage("玩家 【" + li.Name + "】 的所有库存如下:\n" + all + "\n", TextColor());
                        else
                            args.Player.SendMessage("多个结果  玩家 【" + li.Name + "】 的所有库存如下:\n" + all + "\n", TextColor());
                    }
                    else
                        args.Player.SendInfoMessage("玩家 【" + li.Name + "】未携带任何东西\n");
                }
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                foreach (var t in temp)
                {
                    PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                    if (t != null && t2.exists)
                    {
                        users.Add(t, t2);
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendErrorMessage("未查找到该玩家！");
                }
                else if (users.Count == 1)
                {
                    string offAll = GetItemsString(users.First().Value.inventory, users.First().Value.inventory.Length);
                    offAll = FormatArrangement(offAll, 30, " ");
                    if (!string.IsNullOrEmpty(offAll))
                    {
                        args.Player.SendMessage("玩家 【" + users.First().Key.Name + "】 的所有库存如下:" + "\n" + offAll, TextColor());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("玩家 【" + users.First().Key.Name + "】未携带任何东西");
                    }
                }
                else
                {
                    foreach (var p in users)
                    {
                        string offAll = GetItemsString(p.Value.inventory, p.Value.inventory.Length);
                        offAll = FormatArrangement(offAll, 30, " ");
                        if (!string.IsNullOrEmpty(offAll))
                        {
                            args.Player.SendMessage("多个结果  玩家 【" + p.Key.Name + "】 的所有库存如下:" + "\n" + offAll + "\n", TextColor());
                        }
                        else
                        {
                            args.Player.SendInfoMessage("玩家 【" + p.Key.Name + "】 未携带任何东西\n");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 返回文本查阅背包
        /// </summary>
        /// <param name="args"></param>
        private void ViewInventText(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /vit [玩家名]  来查看该玩家的库存");
                return;
            }
            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (list.Any())
            {
                foreach (var li in list)
                {
                    StringBuilder sb = new StringBuilder();
                    string inventory = GetItemsString(li.TPlayer.inventory, NetItem.InventorySlots, 1);

                    string armor = GetItemsString(li.TPlayer.armor, NetItem.ArmorSlots, 1);
                    string armor1 = GetItemsString(li.TPlayer.Loadouts[0].Armor, NetItem.ArmorSlots, 1);
                    string armor2 = GetItemsString(li.TPlayer.Loadouts[1].Armor, NetItem.ArmorSlots, 1);
                    string armor3 = GetItemsString(li.TPlayer.Loadouts[2].Armor, NetItem.ArmorSlots, 1);

                    string dyestuff = GetItemsString(li.TPlayer.dye, NetItem.DyeSlots, 1);
                    string dyestuff1 = GetItemsString(li.TPlayer.Loadouts[0].Dye, NetItem.DyeSlots, 1);
                    string dyestuff2 = GetItemsString(li.TPlayer.Loadouts[1].Dye, NetItem.DyeSlots, 1);
                    string dyestuff3 = GetItemsString(li.TPlayer.Loadouts[2].Dye, NetItem.DyeSlots, 1);

                    string misc = GetItemsString(li.TPlayer.miscEquips, NetItem.MiscEquipSlots, 1);
                    string miscDye = GetItemsString(li.TPlayer.miscDyes, NetItem.MiscDyeSlots, 1);
                    string trash = $" [{Lang.prefix[li.TPlayer.trashItem.prefix].Value}.{li.TPlayer.trashItem.Name}:{li.TPlayer.trashItem.stack}] ";

                    string pig = GetItemsString(li.TPlayer.bank.item, NetItem.PiggySlots, 1);
                    string safe = GetItemsString(li.TPlayer.bank2.item, NetItem.SafeSlots, 1);
                    string forge = GetItemsString(li.TPlayer.bank3.item, NetItem.ForgeSlots, 1);
                    string vault = GetItemsString(li.TPlayer.bank4.item, NetItem.VoidSlots, 1);

                    if (list.Count == 1)
                        sb.AppendLine("玩家 【" + li.Name + "】 的所有库存如下:");
                    else
                        sb.AppendLine("多个结果  玩家 【" + li.Name + "】 的所有库存如下:");

                    if (inventory.Length > 0 && inventory != null && inventory != "")
                    {
                        sb.AppendLine("背包:");
                        sb.AppendLine(FormatArrangement(inventory, 30, " "));
                    }

                    if (armor.Length > 0 && armor != null && armor != "")
                    {
                        sb.AppendLine("盔甲 + 饰品 + 时装:");
                        sb.AppendLine("当前装备栏：");
                        sb.AppendLine(armor);
                    }
                    if (armor1.Length > 0 && armor1 != null && armor1 != "")
                    {
                        sb.AppendLine("装备栏1：");
                        sb.AppendLine(armor1);
                    }
                    if (armor2.Length > 0 && armor2 != null && armor2 != "")
                    {
                        sb.AppendLine("装备栏2：");
                        sb.AppendLine(armor2);
                    }
                    if (armor3.Length > 0 && armor3 != null && armor3 != "")
                    {
                        sb.AppendLine("装备栏3：");
                        sb.AppendLine(armor3);
                    }

                    if (dyestuff.Length > 0 && dyestuff != null && dyestuff != "")
                    {
                        sb.AppendLine("当前染料:");
                        sb.AppendLine(dyestuff);
                    }
                    if (dyestuff1.Length > 0 && dyestuff1 != null && dyestuff1 != "")
                    {
                        sb.AppendLine("染料1:");
                        sb.AppendLine(dyestuff1);
                    }
                    if (dyestuff2.Length > 0 && dyestuff2 != null && dyestuff2 != "")
                    {
                        sb.AppendLine("染料2:");
                        sb.AppendLine(dyestuff2);
                    }
                    if (dyestuff3.Length > 0 && dyestuff3 != null && dyestuff3 != "")
                    {
                        sb.AppendLine("染料3:");
                        sb.AppendLine(dyestuff3);
                    }


                    if (misc.Length > 0 && misc != null && misc != "")
                    {
                        sb.AppendLine("宠物 + 矿车 + 坐骑 + 钩爪:");
                        sb.AppendLine(misc);
                    }
                    if (miscDye.Length > 0 && miscDye != null && miscDye != "")
                    {
                        sb.AppendLine("宠物 矿车 坐骑 钩爪 染料:");
                        sb.AppendLine(miscDye);
                    }
                    if (trash != " [.:0] ")
                    {
                        sb.AppendLine("垃圾桶:");
                        sb.AppendLine(trash);
                    }
                    if (pig.Length > 0 && pig != null && pig != "")
                    {
                        sb.AppendLine("猪猪储蓄罐:");
                        sb.AppendLine(FormatArrangement(pig, 30, " "));
                    }
                    if (safe.Length > 0 && safe != null && safe != "")
                    {
                        sb.AppendLine("保险箱:");
                        sb.AppendLine(FormatArrangement(safe, 30, " "));
                    }
                    if (forge.Length > 0 && forge != null && forge != "")
                    {
                        sb.AppendLine("护卫熔炉:");
                        sb.AppendLine(FormatArrangement(forge, 30, " "));
                    }
                    if (vault.Length > 0 && vault != null && vault != "")
                    {
                        sb.AppendLine("虚空金库:");
                        sb.AppendLine(FormatArrangement(vault, 30, " "));
                    }

                    if (sb.Length > 0 && sb != null && sb.ToString() != "")
                        args.Player.SendMessage(sb.ToString() + "\n", TextColor());
                    else
                        args.Player.SendInfoMessage("玩家【" + li.Name + "】未携带任何东西");
                }
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                foreach (var t in temp)
                {
                    PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                    if (t != null && t2.exists)
                    {
                        users.Add(t, t2);
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendErrorMessage("未查找到该玩家！");
                }
                else if (users.Count == 1)
                {
                    string offAll = GetItemsString(users.First().Value.inventory, users.First().Value.inventory.Length, 1);
                    offAll = FormatArrangement(offAll, 30, " ");
                    if (!string.IsNullOrEmpty(offAll))
                    {
                        args.Player.SendMessage("玩家 【" + users.First().Key.Name + "】 的所有库存如下:" + "\n" + offAll, TextColor());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("玩家 【" + users.First().Key.Name + "】未携带任何东西");
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var p in users)
                    {
                        string offAll = GetItemsString(p.Value.inventory, p.Value.inventory.Length, 1);
                        offAll = FormatArrangement(offAll, 30, " ");
                        if (!string.IsNullOrEmpty(offAll))
                        {
                            sb.AppendLine("多个结果  玩家 【" + p.Key.Name + "】 的所有库存如下:" + "\n" + offAll + "\n");
                        }
                        else
                        {
                            sb.AppendLine("玩家 【" + p.Key.Name + "】未携带任何东西\n");
                        }
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                }
            }
        }


        /// <summary>
        /// 查询玩家的状态
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ViewState(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /vs [玩家名]  来查看该玩家的状态");
                return;
            }
            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);

            if (list.Any())
            {
                StringBuilder sb = new StringBuilder();
                ExtraData? ex = edPlayers.Find(x => x.Name == list[0].Name);
                sb.AppendLine("玩家 【" + list[0].Name + "】 的状态如下:");
                sb.AppendLine("最大生命值[i:29]：" + list[0].TPlayer.statLifeMax + "   当前生命值[i:58]：" + list[0].TPlayer.statLife);
                sb.AppendLine("最大魔力值[i:109]：" + list[0].TPlayer.statManaMax + "   当前魔力值[i:184]：" + list[0].TPlayer.statMana);
                sb.AppendLine("完成渔夫任务数[i:3120]：" + list[0].TPlayer.anglerQuestsFinished);
                sb.AppendLine("库存硬币数[i:855]：" + cointostring(getPlayerCoin(list[0].Account.ID)));
                if (ex != null)
                {
                    sb.AppendLine("在线时长[i:3099]：" + timetostring(ex.time));
                    sb.AppendLine("已击杀生物数[i:3095]：" + ex.killNPCnum + " 个");
                    sb.AppendLine("已击杀Boss[i:3868]：" + DictionaryToVSString(ex.killBossID));
                    sb.AppendLine("已击杀罕见生物[i:4274]：" + DictionaryToVSString(ex.killRareNPCID));
                }

                sb.Append("各种buff[i:678]：");
                int flag = 0;
                foreach (int buff in list[0].TPlayer.buffType)
                {
                    if (buff != 0)
                    {
                        flag++;
                        sb.Append(Lang.GetBuffName(buff) + "  ");
                        if (flag == 12)
                            sb.AppendLine();
                    }
                }
                if (flag == 0)
                {
                    sb.Append("无");
                }
                sb.AppendLine();

                sb.Append("各种永久增益：");
                flag = 0;
                if (list[0].TPlayer.extraAccessory)
                {
                    flag++;
                    sb.Append("[i:3335]  ");
                }
                if (list[0].TPlayer.unlockedBiomeTorches)
                {
                    flag++;
                    sb.Append("[i:5043]  ");
                }
                if (list[0].TPlayer.ateArtisanBread)
                {
                    flag++;
                    sb.Append("[i:5326]  ");
                }
                if (list[0].TPlayer.usedAegisCrystal)
                {
                    flag++;
                    sb.Append("[i:5337]  ");
                }
                if (list[0].TPlayer.usedAegisFruit)
                {
                    flag++;
                    sb.Append("[i:5338]  ");
                }
                if (list[0].TPlayer.usedArcaneCrystal)
                {
                    flag++;
                    sb.Append("[i:5339]  ");
                }
                if (list[0].TPlayer.usedGalaxyPearl)
                {
                    flag++;
                    sb.Append("[i:5340]  ");
                }
                if (list[0].TPlayer.usedGummyWorm)
                {
                    flag++;
                    sb.Append("[i:5341]  ");
                }
                if (list[0].TPlayer.usedAmbrosia)
                {
                    flag++;
                    sb.Append("[i:5342]  ");
                }
                if (list[0].TPlayer.unlockedSuperCart)
                {
                    flag++;
                    sb.Append("[i:5289]");
                }
                if (flag == 0)
                {
                    sb.Append("无");
                }
                sb.AppendLine();
                args.Player.SendMessage(sb.ToString(), TextColor());
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                foreach (var t in temp)
                {
                    PlayerData t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                    if (t != null && t2.exists)
                    {
                        users.Add(t, t2);
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendErrorMessage("未查找到该玩家！");
                }
                else
                {
                    foreach (var p in users)
                    {
                        StringBuilder sb = new StringBuilder();
                        ExtraData? ex = ZPExtraDB.ReadExtraDB(p.Key.ID);
                        if (users.Count == 1)
                            sb.AppendLine("玩家 【" + p.Key.Name + "】 的状态如下:");
                        else
                            sb.AppendLine("多个结果  玩家 【" + p.Key.Name + "】 的状态如下:");

                        sb.AppendLine("最大生命值[i:29]：" + p.Value.maxHealth + "   当前生命值[i:58]：" + p.Value.health);
                        sb.AppendLine("最大魔力值[i:109]：" + p.Value.maxMana + "   当前魔力值[i:184]：" + p.Value.mana);
                        sb.AppendLine("完成渔夫任务数[i:3120]：" + p.Value.questsCompleted);
                        sb.AppendLine("库存硬币数[i:855]：" + cointostring(getPlayerCoin(p.Key.ID)));
                        if (ex != null)
                        {
                            sb.AppendLine("在线时长[i:3099]：" + timetostring(ex.time));
                            sb.AppendLine("已击杀生物数[i:3095]：" + ex.killNPCnum + " 个");
                            sb.AppendLine("已击杀Boss[i:3868]：" + DictionaryToVSString(ex.killBossID));
                            sb.AppendLine("已击杀罕见生物[i:4274]：" + DictionaryToVSString(ex.killRareNPCID));
                        }

                        sb.Append("各种永久增益：");
                        int flag = 0;
                        if (p.Value.extraSlot != null && p.Value.extraSlot.GetValueOrDefault() == 1)
                        {
                            flag++;
                            sb.Append("[i:3335]  ");
                        }
                        if (p.Value.unlockedBiomeTorches == 1)
                        {
                            flag++;
                            sb.Append("[i:5043]  ");
                        }
                        if (p.Value.ateArtisanBread == 1)
                        {
                            flag++;
                            sb.Append("[i:5326]  ");
                        }
                        if (p.Value.usedAegisCrystal == 1)
                        {
                            flag++;
                            sb.Append("[i:5337]  ");
                        }
                        if (p.Value.usedAegisFruit == 1)
                        {
                            flag++;
                            sb.Append("[i:5338]  ");
                        }
                        if (p.Value.usedArcaneCrystal == 1)
                        {
                            flag++;
                            sb.Append("[i:5339]  ");
                        }
                        if (p.Value.usedGalaxyPearl == 1)
                        {
                            flag++;
                            sb.Append("[i:5340]  ");
                        }
                        if (p.Value.usedGummyWorm == 1)
                        {
                            flag++;
                            sb.Append("[i:5341]  ");
                        }
                        if (p.Value.usedAmbrosia == 1)
                        {
                            flag++;
                            sb.Append("[i:5342]  ");
                        }
                        if (p.Value.unlockedSuperCart == 1)
                        {
                            flag++;
                            sb.Append("[i:5289]");
                        }
                        if (flag == 0)
                        {
                            sb.Append("无");
                        }
                        sb.AppendLine();
                        args.Player.SendMessage(sb.ToString(), TextColor());
                    }
                }
            }
        }


        /// <summary>
        /// 查看玩家状态，纯文本
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ViewStateText(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /vst [玩家名]  来查看该玩家的状态，纯文本");
                return;
            }
            string name = args.Parameters[0];
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(name);
            if (list.Any())
            {
                StringBuilder sb = new StringBuilder();
                ExtraData? ex = edPlayers.Find(x => x.Name == list[0].Name);
                sb.AppendLine("玩家 【" + list[0].Name + "】 的状态如下:");
                sb.AppendLine("最大生命值：" + list[0].TPlayer.statLifeMax + "   当前生命值：" + list[0].TPlayer.statLife);
                sb.AppendLine("最大魔力值：" + list[0].TPlayer.statManaMax + "   当前魔力值：" + list[0].TPlayer.statMana);
                sb.AppendLine("完成渔夫任务数：" + list[0].TPlayer.anglerQuestsFinished);
                sb.AppendLine("库存硬币数：" + cointostring(getPlayerCoin(list[0].Account.ID), 1));
                if (ex != null)
                {
                    sb.AppendLine("在线时长：" + timetostring(ex.time));
                    sb.AppendLine("已击杀生物数：" + ex.killNPCnum + " 个");
                    sb.AppendLine("已击杀Boss：" + DictionaryToVSString(ex.killBossID, false));
                    sb.AppendLine("已击杀罕见生物：" + DictionaryToVSString(ex.killRareNPCID, false));
                }

                sb.Append("各种buff：");
                int flag = 0;
                foreach (int buff in list[0].TPlayer.buffType)
                {
                    if (buff != 0)
                    {
                        flag++;
                        sb.Append(Lang.GetBuffName(buff) + "  ");
                    }
                }
                if (flag == 0)
                {
                    sb.Append("无");
                }
                sb.AppendLine();

                sb.Append("各种永久增益：");
                flag = 0;
                if (list[0].TPlayer.extraAccessory)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(3335) + "  ");
                }
                if (list[0].TPlayer.unlockedBiomeTorches)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5043) + "  ");
                }
                if (list[0].TPlayer.ateArtisanBread)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5326) + "  ");
                }
                if (list[0].TPlayer.usedAegisCrystal)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5337) + "  ");
                }
                if (list[0].TPlayer.usedAegisFruit)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5338) + "  ");
                }
                if (list[0].TPlayer.usedArcaneCrystal)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5339) + "  ");
                }
                if (list[0].TPlayer.usedGalaxyPearl)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5340) + "  ");
                }
                if (list[0].TPlayer.usedGummyWorm)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5341) + "  ");
                }
                if (list[0].TPlayer.usedAmbrosia)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5342) + "  ");
                }
                if (list[0].TPlayer.unlockedSuperCart)
                {
                    flag++;
                    sb.Append(Lang.GetItemNameValue(5289));
                }
                if (flag == 0)
                {
                    sb.Append("无");
                }
                sb.AppendLine();
                args.Player.SendMessage(sb.ToString(), TextColor());
            }
            else
            {
                args.Player.SendInfoMessage(offlineplayer);
                Dictionary<UserAccount, PlayerData> users = new Dictionary<UserAccount, PlayerData>();
                List<UserAccount> temp = TShock.UserAccounts.GetUserAccountsByName(name, true);
                foreach (var t in temp)
                {
                    PlayerData t2;
                    if (t != null)
                    {
                        t2 = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), t.ID);
                        if (t2.exists)
                        {
                            users.Add(t, t2);
                        }
                    }
                }
                if (users.Count == 0)
                {
                    args.Player.SendErrorMessage("未查找到该玩家！");
                }
                else
                {
                    foreach (var p in users)
                    {
                        StringBuilder sb = new StringBuilder();
                        ExtraData? ex = ZPExtraDB.ReadExtraDB(p.Key.ID);
                        if (users.Count == 1)
                            sb.AppendLine("玩家 【" + p.Key.Name + "】 的状态如下:");
                        else
                            sb.AppendLine("多个结果  玩家 【" + p.Key.Name + "】 的状态如下:");
                        sb.AppendLine("最大生命值：" + p.Value.maxHealth + "   当前生命值：" + p.Value.health);
                        sb.AppendLine("最大魔力值：" + p.Value.maxMana + "   当前魔力值：" + p.Value.mana);
                        sb.AppendLine("完成渔夫任务数：" + p.Value.questsCompleted);
                        sb.AppendLine("库存硬币数：" + cointostring(getPlayerCoin(p.Key.ID), 1));
                        if (ex != null)
                        {
                            sb.AppendLine("在线时长：" + timetostring(ex.time));
                            sb.AppendLine("已击杀生物数：" + ex.killNPCnum + " 个");
                            sb.AppendLine("已击杀Boss：" + DictionaryToVSString(ex.killBossID, false));
                            sb.AppendLine("已击杀罕见生物：" + DictionaryToVSString(ex.killRareNPCID, false));
                        }
                        sb.Append("各种永久增益：");
                        int flag = 0;
                        if (p.Value.extraSlot != null && p.Value.extraSlot.GetValueOrDefault() == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(3335) + "  ");
                        }
                        if (p.Value.unlockedBiomeTorches == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5043) + "  ");
                        }
                        if (p.Value.ateArtisanBread == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5326) + "  ");
                        }
                        if (p.Value.usedAegisCrystal == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5337) + "  ");
                        }
                        if (p.Value.usedAegisFruit == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5338) + "  ");
                        }
                        if (p.Value.usedArcaneCrystal == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5339) + "  ");
                        }
                        if (p.Value.usedGalaxyPearl == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5340) + "  ");
                        }
                        if (p.Value.usedGummyWorm == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5341) + "  ");
                        }
                        if (p.Value.usedAmbrosia == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5342) + "  ");
                        }
                        if (p.Value.unlockedSuperCart == 1)
                        {
                            flag++;
                            sb.Append(Lang.GetItemNameValue(5289));
                        }
                        if (flag == 0)
                        {
                            sb.Append("无");
                        }
                        sb.AppendLine();
                        args.Player.SendMessage(sb.ToString(), TextColor());
                    }
                }
            }
        }


        /// <summary>
        /// 游戏更新，用来实现人物额外数据如时间的同步增加，这里实现对进服人员的添加和time自增
        /// </summary>
        /// <param name="args"></param>
        private void OnGameUpdate(EventArgs args)
        {//自制计时器，60 Timer = 1 秒
            Timer++;
            //以秒为单位处理，降低计算机计算量
            if (Timer % 60L == 0L)
            {
                //在线时长 +1 的部分，遍历在线玩家，对time进行自增
                TSPlayer[] players = TShock.Players;
                for (int i = 0; i < players.Length; i++)
                {
                    TSPlayer tsp = players[i];
                    if (tsp != null && tsp.IsLoggedIn)
                    {
                        //如果当前玩家已存在，那么更新额外数据
                        ExtraData? extraData = edPlayers.Find((ExtraData x) => x.Name == tsp.Name);
                        if (extraData != null)
                        {
                            extraData.time += 1L;
                            if (extraData.time % 1800L == 0L)
                            {
                                tsp.SendMessage("您已经在线了 " + timetostring(extraData.time), broadcastColor);
                                TShock.Log.Info("玩家 " + extraData.Name + " 已经在线了 " + timetostring(extraData.time));
                                NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(tsp.TPlayer.Center, 4), tsp.Index, -1);
                                Projectile.NewProjectile(null, tsp.TPlayer.Center, -Vector2.UnitY * 4f, Main.rand.Next(415, 419), 0, 0f, -1, 0f, 0f, 0f);
                            }
                        }
                        //否则查找是否已注册过
                        else
                        {
                            //注册过了，那么读取
                            ExtraData? extraData2 = ZPExtraDB.ReadExtraDB(tsp.Account.ID);
                            if (extraData2 != null)
                            {
                                edPlayers.Add(extraData2);
                            }
                            //否则创建一个新的
                            else
                            {
                                ExtraData ex = new ExtraData(tsp.Account.ID, tsp.Name, 0L, 25, 0, new Dictionary<int, int>(), new Dictionary<int, int>());
                                ZPExtraDB.WriteExtraDB(ex);
                                edPlayers.Add(ex);
                            }
                        }
                    }
                }


                //每10分钟自动将额外数据库保存到数据库一次
                if (Timer % 36000L == 0L)
                {
                    foreach (ExtraData ex in edPlayers)
                    {
                        ZPExtraDB.WriteExtraDB(ex);
                    }
                }


                //自动备份的处理部分，这里以分钟为单位
                if (Timer % 3600L == 0L)
                {
                    foreach (var ex in edPlayers)
                    {//到达备份间隔时长，备份一次
                        foreach (TSPlayer ts in TShock.Players)
                        {
                            if(ts == null || !ts.IsLoggedIn || ts.Name != ex.Name)
                            {
                                continue;
                            }
                            if (ex.backuptime != 0L && Timer % (3600L * ex.backuptime) == 0L)
                            {
                                ZPExtraDB.WriteExtraDB(ex);
                                ZPDataBase.AddZPlayerDB(ts);
                                ts.SendMessage("已自动备份您的人物存档和额外数据", new Color(0, 255, 0));
                            }
                        }
                    }
                }


                //自动备份的处理部分，这里以分钟为单位
                if (Timer % 3600L == 0L)
                {
                    foreach (var ex in edPlayers)
                    {//到达备份间隔时长，备份一次
                        foreach (TSPlayer ts in TShock.Players)
                        {
                            if (ts == null || !ts.IsLoggedIn || ts.Name != ex.Name)
                            {
                                continue;
                            }
                            if (ex.backuptime != 0L && Timer % (3600L * ex.backuptime) == 0L)
                            {
                                ZPExtraDB.WriteExtraDB(ex);
                                ZPDataBase.AddZPlayerDB(ts);
                                ts.SendMessage("已自动备份您的人物存档和额外数据", new Color(0, 255, 0));
                            }
                        }
                    }
                }
            }

            //冻结处理
            if (Timer % 5L == 0L)
            {
                if (frePlayers.Count != 0)
                {
                    foreach (var v in frePlayers)
                    {
                        TShock.Players.ForEach(x =>
                        {
                            if (x != null && x.IsLoggedIn && (x.UUID.Equals(v.uuid) || x.Name.Equals(v.name) || !string.IsNullOrEmpty(v.IPs) && !string.IsNullOrEmpty(x.IP) && IPStostringIPs(v.IPs).Contains(x.IP)))
                            {
                                x.SetBuff(149, 720);//网住
                                x.SetBuff(156, 720);//石化
                                x.SetBuff(47, 300); //冰冻
                                x.SetBuff(23, 300); //诅咒
                                x.SetBuff(31, 300); //困惑
                                x.SetBuff(80, 300); //灯火管制
                                x.SetBuff(88, 300); //混沌
                                x.SetBuff(120, 300);//臭气
                                x.SetBuff(145, 300);//月食
                                x.SetBuff(163, 300);//阻塞
                                x.SetBuff(199, 300);//创意震撼
                                x.SetBuff(160, 300);//眩晕
                                x.SetBuff(197, 300);//粘液
                                if (Timer % 240L == 0)
                                {
                                    x.SendInfoMessage("您已被冻结，详情请询问管理员");
                                    SendText(x, "您已被冻结", Color.Red, x.TPlayer.Center);
                                }
                            }
                        });
                    }
                }
            }
        }


        /// <summary>
        /// 对进入服务器的玩家进行一些限制
        /// </summary>
        /// <param name="args"></param>
        private void OnServerJoin(JoinEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
            {
                return;
            }
            TSPlayer tsplayer = TShock.Players[args.Who];
            if (int.TryParse(tsplayer.Name, out int num))
            {
                tsplayer.Kick("请不要起纯数字名字", true);
                return;
            }
            if ((tsplayer.Name[0] >= ' ' && tsplayer.Name[0] <= '/') || (tsplayer.Name[0] >= ':' && tsplayer.Name[0] <= '@') || (tsplayer.Name[0] > '[' && tsplayer.Name[0] <= '`') || (tsplayer.Name[0] >= '{' && tsplayer.Name[0] <= '~'))
            {
                tsplayer.Kick("请不要在名字中使用特殊符号", true);
                return;
            }
            if (tsplayer.Name.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("你的名字含有指令关键字: all ，请更换", true);
            }
            else if (tsplayer.Name.Equals("time", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("你的名字含有指令关键字: time ，请更换", true);
            }
            else if (tsplayer.Name.Equals("fish", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("你的名字含有指令关键字: fish ，请更换", true);
            }
            else if (tsplayer.Name.Equals("coin", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("你的名字含有指令关键字: coin ，请更换", true);
            }
            else if (tsplayer.Name.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                tsplayer.Kick("你的名字含有指令关键字: help ，请更换", true);
            }
        }


        /// <summary>
        /// 对离开服务区的玩家的额外数据库，进行保存
        /// </summary>
        /// <param name="args"></param>
        private void OnServerLeave(LeaveEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
            {
                return;
            }
            //清理掉这个离开服务器的玩家的额外数据内存
            foreach (var v in edPlayers)
            {
                if (v.Name == TShock.Players[args.Who].Name)
                {
                    ZPExtraDB.WriteExtraDB(v);
                    edPlayers.RemoveAll(x => x.Account == v.Account || x.Name == v.Name);
                    break;
                }
            }
            //顺便遍历下整个edplayers，移除所有和tsplayers不同步的元素
            for (int i = 0; i < edPlayers.Count; i++)
            {
                bool flag = false;
                foreach (TSPlayer p in TShock.Players)
                {
                    if (p != null && p.IsLoggedIn && (p.Name == edPlayers[i].Name || p.Account.ID == edPlayers[i].Account))
                    {
                        flag = true; break;
                    }
                }
                if (!flag)
                {
                    ZPExtraDB.WriteExtraDB(edPlayers[i]);
                    edPlayers.RemoveAt(i);
                    i--;
                }
            }
        }


        /// <summary>
        /// 导出这个玩家的人物存档
        /// </summary>
        /// <param name="args"></param>
        private void ZhiExportPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zout [name]  来导出该玩家的人物存档\n输入 /zout all  来导出所有人物的存档");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Dictionary<UserAccount, PlayerData> players = new Dictionary<UserAccount, PlayerData>();
                    using (QueryResult queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter"))
                    {
                        while (queryResult.Read())
                        {
                            int num = queryResult.Get<int>("Account");
                            UserAccount user = TShock.UserAccounts.GetUserAccountByID(num);
                            players.Add(user, TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), num));
                        }
                    }
                    args.Player.SendMessage("预计导出所有用户存档数目：" + players.Count, new Color(100, 233, 255));
                    TShock.Log.Info("预计导出所有用户存档数目：" + players.Count.ToString());
                    StringBuilder sb = new StringBuilder();
                    foreach (var one in players)
                    {
                        Player? player = CreateAPlayer(one.Key.Name, one.Value);
                        if (ExportPlayer(player, ZPExtraDB.getPlayerExtraDBTime(one.Key.ID)))
                        {
                            if (args.Player.IsLoggedIn)
                            {
                                args.Player.SendMessage($"用户 [{player!.name}] 已导出，目录：tshock / ZhiPlayer / {Main.worldName} / {player!.name}.plr", new Color(0, 255, 0));
                            }
                            else
                            {
                                sb.AppendLine($"用户 [{player!.name}] 已导出，目录：tshock / ZhiPlayer / {Main.worldName} / {player!.name}.plr");
                            }
                            TShock.Log.Info($"用户 [{player!.name}] 已导出，目录：tshock / ZhiPlayer / {Main.worldName} / {player!.name}.plr");
                        }
                        else
                        {
                            if (args.Player.IsLoggedIn)
                            {
                                args.Player.SendInfoMessage("用户 [" + one.Key + "] 因数据残缺导出失败");
                            }
                            else
                            {
                                sb.AppendLine($"用户 [{one.Key.Name}] 因数据残缺导出失败");
                            }
                            TShock.Log.Info($"用户 [{one.Key.Name}] 因数据残缺导出失败");
                        }
                    }
                    if (!args.Player.IsLoggedIn)
                    {
                        args.Player.SendInfoMessage(sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误 ZhiExportPlayer ：" + ex.ToString());
                    args.Player.SendErrorMessage("错误 ZhiExportPlayer ：" + ex.ToString());
                    Console.WriteLine("错误 ZhiExportPlayer ：" + ex.ToString());
                }
                return;
            }
            List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (list.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                if (user == null)
                {
                    if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                        return;
                    }
                    else if (users.Count > 1)
                    {
                        args.Player.SendInfoMessage(manyplayer);
                        return;
                    }
                    else
                        user = users[0];
                }
                PlayerData playerData = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), user.ID);
                Player? player = CreateAPlayer(args.Parameters[0], playerData);
                if (ExportPlayer(player, ZPExtraDB.getPlayerExtraDBTime(user.ID)))
                {
                    args.Player.SendMessage($"导出成功！目录：tshock / ZhiPlayer / {Main.worldName} / {args.Parameters[0]}.plr", new Color(0, 255, 0));
                    TShock.Log.Info($"导出成功！目录：tshock / ZhiPlayer / {Main.worldName} / {args.Parameters[0]}.plr");
                }
                else
                {
                    args.Player.SendErrorMessage("导出失败，因数据残缺");
                    TShock.Log.Info("导出失败，因数据残缺");
                }
            }
            else if (list.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else if (ExportPlayer(list[0].TPlayer, ZPExtraDB.getPlayerExtraDBTime(list[0].Account.ID)))
            {
                args.Player.SendMessage($"导出成功！目录：tshock / ZhiPlayer / {Main.worldName} / {list[0].Name}.plr", new Color(0, 255, 0));
                TShock.Log.Info($"导出成功！目录：tshock / ZhiPlayer / {Main.worldName} / {list[0].Name}.plr");
            }
            else
            {
                args.Player.SendErrorMessage("导出失败，因数据残缺");
                TShock.Log.Info("导出失败，因数据残缺");
            }
        }


        /// <summary>
        /// 对玩家在线时常进行排序
        /// </summary>
        /// <param name="args"></param>
        private void ZhiSortPlayer(CommandArgs args)
        {
            if (args.Parameters.Count != 1 && args.Parameters.Count != 2)
            {
                args.Player.SendInfoMessage("输入 /zsort help  来查看排序系列指令帮助");
                return;
            }
            //帮助指令
            if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                args.Player.SendInfoMessage(
                    "输入 /zsort time  来查看人物在线时间排行榜前十名\n" +
                    "输入 /zsort time [num]  来查看当前[num]个人物在线时间排行榜\n" +
                    "输入 /zsort time all  来查看所有玩家在线时常排行榜\n" +
                    "输入 /zsort coin  来查看人物硬币数目排行榜前十名\n" +
                    "输入 /zsort coin [num]  来查看当前[num]个人物硬币数目排行榜\n" +
                    "输入 /zsort coin all  来查看所有玩家硬币数目排行榜\n" +
                    "输入 /zsort fish  来查看人物任务鱼数目排行榜前十名\n" +
                    "输入 /zsort fish [num]  来查看当前[num]个人物任务鱼数目排行榜\n" +
                    "输入 /zsort fish all  来查看所有玩家任务鱼数目排行榜\n" +
                    "输入 /zsort kill [num]  来查看当前[num]个人物击杀生物数排行榜\n" +
                    "输入 /zsort kill  来查看人物击杀生物数排行榜前十名\n" +
                    "输入 /zsort kill all  来查看所有玩家击杀生物数排行榜\n" +
                    "输入 /zsort boss [num]  来查看当前[num]个人物击杀Boss总数排行榜\n" +
                    "输入 /zsort boss  来查看人物击杀Boss总数排行榜前十名\n" +
                    "输入 /zsort boss all  来查看所有玩家击杀Boss总数排行榜\n" +
                    "输入 /zsort rarenpc [num]  来查看当前[num]个人物击杀罕见生物总数排行榜\n" +
                    "输入 /zsort rarenpc  来查看人物击杀罕见生物总数排行榜前十名\n" +
                    "输入 /zsort rarenpc all  来查看所有玩家击杀罕见生物总数排行榜"
                    );
                return;
            }
            //时间排序
            else if (args.Parameters[0].Equals("time", StringComparison.OrdinalIgnoreCase))
            {
                // time 排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();
                list.Sort((p1, p2) => p2.time.CompareTo(p1.time));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 在线时长 {timetostring(list[i].time)}");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("数字无效");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("当前最多 " + list.Count + " 人");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 在线时长 {timetostring(list[i].time)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 在线时长 {timetostring(list[i].time)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("输入 /zsort time [num]  来查看当前[num]个人物在线时间排行榜\n输入 /zsort time  来查看人物在线时间排行榜前十名\n输入 /zsort time all  来查看所有玩家在线时常排行榜");
                    }
                }
            }
            //钱币排序
            else if (args.Parameters[0].Equals("coin", StringComparison.OrdinalIgnoreCase))
            {
                List<UserAccount> list = new List<UserAccount>();
                using (QueryResult queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter"))
                {
                    while (queryResult.Read())
                    {
                        int num = queryResult.Get<int>("Account");
                        list.Add(TShock.UserAccounts.GetUserAccountByID(num));
                    }
                }

                list.Sort((p1, p2) => getPlayerCoin(p2.ID).CompareTo(getPlayerCoin(p1.ID)));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 总硬币数 {cointostring(getPlayerCoin(list[i].ID), 1)}");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("数字无效");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("当前最多 " + list.Count + " 人");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 总硬币数 {cointostring(getPlayerCoin(list[i].ID), 1)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 总硬币数 {cointostring(getPlayerCoin(list[i].ID), 1)}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("输入 /zsort coin  来查看人物硬币数目排行榜前十名\n输入 /zsort coin [num]  来查看当前[num]个人物硬币数目排行榜\n输入 /zsort coin all  来查看所有玩家硬币数目排行榜");
                    }
                }
            }
            //钓鱼任务排序
            else if (args.Parameters[0].Equals("fish", StringComparison.OrdinalIgnoreCase))
            {
                List<UserAccount> list = new List<UserAccount>();
                using (QueryResult queryResult = TShock.DB.QueryReader("SELECT * FROM tsCharacter"))
                {
                    while (queryResult.Read())
                    {
                        int num = queryResult.Get<int>("Account");
                        list.Add(TShock.UserAccounts.GetUserAccountByID(num));
                    }
                }

                list.Sort((p1, p2) => TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), p2.ID).questsCompleted.CompareTo(TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), p1.ID).questsCompleted));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 总完成任务鱼数 {TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), list[i].ID).questsCompleted}");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("数字无效");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("当前最多 " + list.Count + " 人");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 总完成任务鱼数 {TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), list[i].ID).questsCompleted}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 总完成任务鱼数 {TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), list[i].ID).questsCompleted}");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("输入 /zsort fish  来查看人物任务鱼数目排行榜前十名\n输入 /zsort fish [num]  来查看当前[num]个人物任务鱼数目排行榜\n输入 /zsort fish all  来查看所有玩家任务鱼数目排行榜");
                    }
                }
            }
            //斩杀数排序
            else if (args.Parameters[0].Equals("kill", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();
                list.Sort((p1, p2) => p2.killNPCnum.CompareTo(p1.killNPCnum));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀生物总数 {list[i].killNPCnum} 个");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("数字无效");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("当前最多 " + list.Count + " 人");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀生物总数 {list[i].killNPCnum} 个");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀生物总数 {list[i].killNPCnum} 个");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("输入 /zsort kill [num]  来查看当前[num]个人物击杀生物数排行榜\n输入 /zsort kill  来查看人物击杀生物数排行榜前十名\n输入 /zsort kill all  来查看所有玩家击杀生物数排行榜");
                    }
                }
            }
            //斩杀Boss排序
            else if (args.Parameters[0].Equals("boss", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();
                list.Sort((p1, p2) => getKillNumFromDictionary(p2.killBossID).CompareTo(getKillNumFromDictionary(p1.killBossID)));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀Boss总数 {getKillNumFromDictionary(list[i].killBossID)} 个");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("数字无效");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("当前最多 " + list.Count + " 人");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀Boss总数 {getKillNumFromDictionary(list[i].killBossID)} 个");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀Boss总数 {getKillNumFromDictionary(list[i].killBossID)} 个");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("输入 /zsort boss [num]  来查看当前[num]个人物击杀Boss总数排行榜\n输入 /zsort boss  来查看人物击杀Boss总数排行榜前十名\n输入 /zsort boss all  来查看所有玩家击杀Boss总数排行榜");
                    }
                }
            }
            //斩杀罕见生物排序
            else if (args.Parameters[0].Equals("rarenpc", StringComparison.OrdinalIgnoreCase))
            {   //排序前先保存
                foreach (ExtraData ex in edPlayers)
                {
                    ZPExtraDB.WriteExtraDB(ex);
                }
                List<ExtraData> list = ZPExtraDB.ListAllExtraDB();
                list.Sort((p1, p2) => getKillNumFromDictionary(p2.killRareNPCID).CompareTo(getKillNumFromDictionary(p1.killRareNPCID)));
                if (args.Parameters.Count == 1)
                {
                    int num = 10;
                    if (num > list.Count)
                    {
                        num = list.Count;
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < num; i++)
                    {
                        sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀罕见生物总数 {getKillNumFromDictionary(list[i].killRareNPCID)} 个");
                    }
                    args.Player.SendMessage(sb.ToString(), TextColor());
                    TShock.Log.Info(sb.ToString());
                }
                else
                {
                    if (int.TryParse(args.Parameters[1], out int count))
                    {
                        if (count <= 0)
                        {
                            args.Player.SendInfoMessage("数字无效");
                            return;
                        }
                        StringBuilder sb = new StringBuilder();
                        if (count > list.Count)
                        {
                            sb.AppendLine("当前最多 " + list.Count + " 人");
                            count = list.Count;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀罕见生物总数 {getKillNumFromDictionary(list[i].killRareNPCID)} 个");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < list.Count; i++)
                        {
                            sb.AppendLine($"第 {i + 1} 名: 【{list[i].Name}】 击杀罕见生物总数 {getKillNumFromDictionary(list[i].killRareNPCID)} 个");
                        }
                        args.Player.SendMessage(sb.ToString(), TextColor());
                        TShock.Log.Info(sb.ToString());
                    }
                    else
                    {
                        args.Player.SendInfoMessage("输入 /zsort rarenpc [num]  来查看当前[num]个人物击杀罕见生物总数排行榜\n输入 /zsort rarenpc  来查看人物击杀罕见生物总数排行榜前十名\n输入 /zsort rarenpc all  来查看所有玩家击杀罕见生物总数排行榜");
                    }
                }
            }

            else
            {
                args.Player.SendInfoMessage("输入 /zsort help  来查看排序系列指令帮助");
            }
        }


        /// <summary>
        /// 办掉离线或在线的玩家，超级ban指令
        /// </summary>
        /// <param name="args"></param>
        private void SuperBan(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendInfoMessage("输入 /zban add [name]  来封禁无论是否在线的玩家");
                return;
            }
            if (args.Parameters[0].Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                List<TSPlayer> list = BestFindPlayerByNameOrIndex(args.Parameters[1]);
                if (list.Count == 1)
                {
                    if (args.Parameters.Count == 3)
                        list[0].Ban(args.Parameters[2], "ZHIPlayer by " + args.Player.Name);
                    else
                        list[0].Ban("好好反思你干了什么！", "ZHIPlayer by " + args.Player.Name);
                }
                else if (list.Count > 1)
                {
                    args.Player.SendInfoMessage(manyplayer);
                    return;
                }
                else
                {
                    args.Player.SendInfoMessage(offlineplayer);
                    string reason;
                    UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                    if (user != null)
                    {
                        if (args.Parameters.Count == 3)
                        {
                            reason = args.Parameters[2];
                        }
                        else
                        {
                            reason = "好好反思你干了什么！";
                        }
                        TShock.Bans.InsertBan("acc:" + user.Name, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                        if(!string.IsNullOrWhiteSpace(user.UUID))
                            TShock.Bans.InsertBan("uuid:" + user.UUID, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                        if (!string.IsNullOrWhiteSpace(user.KnownIps))
                        {
                            string[] ips = IPStostringIPs(user.KnownIps);
                            foreach (string str in ips)
                            {
                                TShock.Bans.InsertBan("ip:" + str, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                            }
                        }
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendMessage($"用户 {user.Name} 已被 {args.Player.Name} 封禁，指令来源ZHIPlayerManager插件", broadcastColor);
                        TSPlayer.All.SendMessage($"用户 {user.Name} 已被 {args.Player.Name} 封禁，指令来源ZHIPlayerManager插件", broadcastColor);
                        TShock.Log.Info($"用户 {user.Name} 已被 {args.Player.Name} 封禁，指令来源ZHIPlayerManager插件");
                    }
                    else
                    {
                        args.Player.SendInfoMessage("精准查找未找到，正在尝试模糊查找");
                        List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[1], true);
                        if (users.Count == 1)
                        {
                            if (args.Parameters.Count == 3)
                            {
                                reason = args.Parameters[2];
                            }
                            else
                            {
                                reason = "好好反思你干了什么！";
                            }
                            TShock.Bans.InsertBan("acc:" + users[0].Name, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                            if (!string.IsNullOrWhiteSpace(users[0].UUID))
                                TShock.Bans.InsertBan("uuid:" + users[0].UUID, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                            if (!string.IsNullOrWhiteSpace(users[0].KnownIps))
                            {
                                string[] ips = IPStostringIPs(users[0].KnownIps);
                                foreach (string str in ips)
                                {
                                    TShock.Bans.InsertBan("ip:" + str, reason, "ZHIPlayerManager by " + args.Player.Name, DateTime.UtcNow, DateTime.MaxValue);
                                }
                            }
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendMessage($"用户 {users[0].Name} 已被 {args.Player.Name} 封禁，指令来源ZHIPlayerManager插件", broadcastColor);
                            TSPlayer.All.SendMessage($"用户 {users[0].Name} 已被 {args.Player.Name} 封禁，指令来源ZHIPlayerManager插件", broadcastColor);
                            TShock.Log.Info($"用户 {users[0].Name} 已被 {args.Player.Name} 封禁，指令来源ZHIPlayerManager插件");
                        }
                        else if (users.Count > 1)
                        {
                            args.Player.SendInfoMessage("人数不唯一，为避免误封，请重新输入，若玩家名称带有空格可用英文引号括起来");
                        }
                        else
                            args.Player.SendInfoMessage(noplayer);
                    }
                }
            }
            else
            {
                args.Player.SendInfoMessage("输入 /zban add [name]  来封禁无论是否在线的玩家");
            }
        }


        /// <summary>
        /// 冻结该玩家，禁止他做出任何操作
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ZFreeze(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zfre [name]  来冻结该玩家");
                return;
            }
            List<TSPlayer> ts = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (ts.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user != null)
                {
                    frePlayers.Add(new MessPlayer(user.ID, user.Name, user.UUID, user.KnownIps));
                    args.Player.SendMessage($"玩家 [{user.Name}] 已冻结", new Color(0, 255, 0));
                }
                else
                {
                    args.Player.SendInfoMessage("精准查找未找到，正在尝试模糊查找");
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (users.Count > 1)
                    {
                        args.Player.SendInfoMessage(manyplayer);
                        return;
                    }
                    else if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                        return;
                    }
                    else
                    {
                        frePlayers.Add(new MessPlayer(users[0].ID, users[0].Name, users[0].UUID, users[0].KnownIps));
                        args.Player.SendMessage($"玩家 [{users[0].Name}] 已冻结", new Color(0, 255, 0));
                    }
                }
            }
            else if (ts.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else
            {
                frePlayers.Add(new MessPlayer(ts[0].Account.ID, ts[0].Name, ts[0].UUID, ts[0].Account.KnownIps));
                args.Player.SendMessage($"玩家 [{ts[0].Name}] 已冻结", new Color(0, 255, 0));
            }
        }


        /// <summary>
        /// 取消冻结该玩家
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ZUnFreeze(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendInfoMessage("输入 /zunfre [name]  来解冻该玩家\n输入 /zunfre all  来解冻所有玩家");
                return;
            }
            if (args.Parameters[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                frePlayers.ForEach(x =>
                {
                    List<TSPlayer> ts = BestFindPlayerByNameOrIndex(x.name);
                    if (ts.Count > 0 && ts[0].Name == x.name)
                    {
                        clearAllBuffFromPlayer(ts[0]);
                    }
                });
                frePlayers.Clear();
                args.Player.SendMessage("所有玩家均已解冻", new Color(0, 255, 0));
                return;
            }
            List<TSPlayer> ts = BestFindPlayerByNameOrIndex(args.Parameters[0]);
            if (ts.Count == 0)
            {
                args.Player.SendInfoMessage(offlineplayer);
                UserAccount user = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);
                if (user != null)
                {
                    frePlayers.RemoveAll(x => x.uuid == user.UUID || x.name == user.Name || !string.IsNullOrEmpty(x.IPs) && !string.IsNullOrEmpty(user.KnownIps) && IPStostringIPs(x.IPs).Any(x => IPStostringIPs(user.KnownIps).Contains(x)));
                    args.Player.SendMessage($"玩家 [{user.Name}] 已解冻", new Color(0, 255, 0));
                }
                else
                {
                    args.Player.SendInfoMessage("精准查找未找到，正在尝试模糊查找");
                    List<UserAccount> users = TShock.UserAccounts.GetUserAccountsByName(args.Parameters[0], true);
                    if (users.Count > 1)
                    {
                        args.Player.SendInfoMessage(manyplayer);
                        return;
                    }
                    else if (users.Count == 0)
                    {
                        args.Player.SendInfoMessage(noplayer);
                        return;
                    }
                    else
                    {
                        frePlayers.RemoveAll(x => x.uuid == users[0].UUID || x.name == users[0].Name || !string.IsNullOrEmpty(x.IPs) && !string.IsNullOrEmpty(users[0].KnownIps) && IPStostringIPs(x.IPs).Any(x => IPStostringIPs(users[0].KnownIps).Contains(x)));
                        args.Player.SendMessage($"玩家 [{users[0].Name}] 已解冻", new Color(0, 255, 0));
                    }
                }
            }
            else if (ts.Count > 1)
            {
                args.Player.SendInfoMessage(manyplayer);
            }
            else
            {
                frePlayers.RemoveAll(x => x.uuid == ts[0].UUID || x.name == ts[0].Name || !string.IsNullOrEmpty(x.IPs) && !string.IsNullOrEmpty(ts[0].IP) && IPStostringIPs(x.IPs).Any(x => ts[0].IP == x));
                clearAllBuffFromPlayer(ts[0]);
                args.Player.SendMessage($"玩家 [{ts[0].Name}] 已解冻", new Color(0, 255, 0));
            }
        }


        /// <summary>
        /// 击中npc时进行标记
        /// </summary>
        /// <param name="args"></param>
        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (strikeNPC.Exists(x => x.index == args.Npc.whoAmI && x.id == args.Npc.netID && x.players.Contains(args.Player.name))
                || args.Player == null || args.Npc.netID == 488 || args.Npc.lifeMax == 1 || args.Npc.townNPC)
            {
                return;
            }
            //strikeNPC.RemoveAll(x => x.index == args.Npc.whoAmI && (x.id != args.Npc.netID));
            StrikeNPC? strike = strikeNPC.Find(x => x.index == args.Npc.whoAmI && x.id == args.Npc.netID);
            if (strike != null)
            {
                strike.players.Add(args.Player.name);
            }
            else
            {
                StrikeNPC snpc = new StrikeNPC();
                snpc.id = args.Npc.netID;
                snpc.index = args.Npc.whoAmI;
                snpc.name = args.Npc.FullName;

                switch (snpc.id)
                {
                    case 13://世界吞噬者
                    case 325://哀木
                    case 327://南瓜王
                    case 564://T1黑暗魔法师
                    case 565://T3黑暗魔法师
                    case 576://T2食人魔
                    case 577://T3食人魔
                    case 551://双足翼龙
                    //case 491://荷兰飞船 492大炮
                    case 344://常绿尖叫怪
                    case 345://冰雪女皇
                    case 346://圣诞坦克
                    case 517://日耀柱
                    case 422://星璇柱
                    case 493://星尘柱
                    case 507://星云柱
                        snpc.isBoss = true;
                        break;
                    default:
                        snpc.isBoss = args.Npc.boss;
                        break;
                }

                snpc.players.Add(args.Player.name);
                strikeNPC.Add(snpc);
            }
        }


        /// <summary>
        /// 杀死npc时计数
        /// </summary>
        /// <param name="args"></param>
        private void OnNPCKilled(NpcKilledEventArgs args)
        {
            //对被击杀的生物进行计数
            for (int i = 0; i < strikeNPC.Count; i++)
            {
                if (strikeNPC[i].index == args.npc.whoAmI && strikeNPC[i].id == args.npc.netID)
                {
                    //世界吞噬者特殊处理
                    if (strikeNPC[i].id == 13)
                    {
                        bool flag = true;
                        foreach (var n in Main.npc)
                        {
                            if (n.whoAmI != args.npc.whoAmI && n.type == 13 && n.active)
                            {
                                flag = false; break;
                            }
                        }
                        if (flag)
                        {
                            edPlayers.ForEach(x =>
                            {
                                if (strikeNPC[i].players.Contains(x.Name))
                                {
                                    x.killNPCnum++;
                                    if (x.killBossID.TryGetValue(strikeNPC[i].id, out int value))
                                        x.killBossID[strikeNPC[i].id]++;
                                    else
                                        x.killBossID.Add(strikeNPC[i].id, 1);
                                    List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                    if (temp.Count != 0)
                                    {
                                        SendText(temp[0], "击杀 + 1", Color.White, args.npc.Center);
                                        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                    }
                                }
                            });
                            strikeNPC.RemoveAt(i);
                            i--;
                        }
                    }
                    //荷兰飞船的处理
                    else if (strikeNPC[i].id == 492)
                    {
                        bool flag = true;
                        foreach (var n in Main.npc)
                        {
                            if (n.whoAmI != args.npc.whoAmI && n.type == 492 && n.active)
                            {
                                flag = false; break;
                            }
                        }
                        if (flag)
                        {
                            edPlayers.ForEach(x =>
                            {
                                if (strikeNPC[i].players.Contains(x.Name))
                                {
                                    x.killNPCnum += 2;
                                    if (x.killBossID.TryGetValue(strikeNPC[i].id - 1, out int value))
                                        x.killBossID[strikeNPC[i].id - 1]++;
                                    else
                                        x.killBossID.Add(strikeNPC[i].id - 1, 1);
                                    List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                    if (temp.Count != 0)
                                    {
                                        SendText(temp[0], "击杀 + 1", Color.White, args.npc.Center);
                                        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                    }
                                }
                            });
                            strikeNPC.RemoveAt(i);
                            i--;
                        }
                    }
                    //寻常生物的处理
                    else
                    {
                        edPlayers.ForEach(x =>
                        {
                            if (strikeNPC[i].players.Contains(x.Name))
                            {
                                x.killNPCnum++;
                                if (strikeNPC[i].isBoss)
                                {
                                    if (x.killBossID.TryGetValue(strikeNPC[i].id, out int value))
                                        x.killBossID[strikeNPC[i].id]++;
                                    else
                                        x.killBossID.Add(strikeNPC[i].id, 1);
                                }
                                if (args.npc.rarity > 0 && !args.npc.SpawnedFromStatue || args.npc.netID == 87)
                                {
                                    if (x.killRareNPCID.TryGetValue(strikeNPC[i].id, out int value))
                                        x.killRareNPCID[strikeNPC[i].id]++;
                                    else
                                        x.killRareNPCID.Add(strikeNPC[i].id, 1);
                                }
                                List<TSPlayer> temp = BestFindPlayerByNameOrIndex(x.Name);
                                if (temp.Count != 0)
                                {
                                    SendText(temp[0], "击杀 + 1", Color.White, args.npc.Center);
                                    if (strikeNPC[i].isBoss)
                                        NetMessage.PlayNetSound(new NetMessage.NetSoundInfo(temp[0].TPlayer.Center, 4), temp[0].Index, -1);
                                }
                            }
                        });
                        strikeNPC.RemoveAt(i);
                        i--;
                    }
                }
                //清理因为意外导致的不正确的数据
                if (i >= 0 && (strikeNPC[i].id != Main.npc[strikeNPC[i].index].netID || !Main.npc[strikeNPC[i].index].active))
                {
                    strikeNPC.RemoveAt(i);
                    i--;
                }
            }
            //strikeNPC.RemoveAll(x => x.id != Main.npc[x.index].netID || !Main.npc[x.index].active);
            /*
            TSPlayer.All.SendInfoMessage("数目：" + strikeNPC.Count);
            if (strikeNPC.Count > 0)
            {
                foreach (var v in strikeNPC)
                {
                    TSPlayer.All.SendInfoMessage($"name:{v.name}, id:{v.id}, index:{v.index}, player:{v.players.ToArray()}, main.id:{Main.npc[v.index].netID}, active:{Main.npc[v.index].active}");
                }
            }
            */
        }
    }
}