using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace ZHIPlayerManager
{
    public partial class ZHIPM : TerrariaPlugin
    {
        public class ZplayerDB
        {
            private IDbConnection database;

            private string tableName;

            private readonly int MaxSlot = 5;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="db"></param>
            public ZplayerDB(IDbConnection db)
            {
                database = db;
                tableName = "Zhipm_PlayerBackUp";
                SqlTable table = new SqlTable(tableName, new SqlColumn[]
                {
                    new SqlColumn("AccAndSlot", MySqlDbType.Text)
                    {
                        Primary = true
                    },
                    new SqlColumn("Account", MySqlDbType.Int32),
                    new SqlColumn("Name", MySqlDbType.Text),
                    new SqlColumn("Health", MySqlDbType.Int32),
                    new SqlColumn("MaxHealth", MySqlDbType.Int32),
                    new SqlColumn("Mana", MySqlDbType.Int32),
                    new SqlColumn("MaxMana", MySqlDbType.Int32),
                    new SqlColumn("Inventory", MySqlDbType.Text),
                    new SqlColumn("extraSlot", MySqlDbType.Int32),
                    new SqlColumn("spawnX", MySqlDbType.Int32),
                    new SqlColumn("spawnY", MySqlDbType.Int32),
                    new SqlColumn("skinVariant", MySqlDbType.Int32),
                    new SqlColumn("hair", MySqlDbType.Int32),
                    new SqlColumn("hairDye", MySqlDbType.Int32),
                    new SqlColumn("hairColor", MySqlDbType.Int32),
                    new SqlColumn("pantsColor", MySqlDbType.Int32),
                    new SqlColumn("shirtColor", MySqlDbType.Int32),
                    new SqlColumn("underShirtColor", MySqlDbType.Int32),
                    new SqlColumn("shoeColor", MySqlDbType.Int32),
                    new SqlColumn("hideVisuals", MySqlDbType.Int32),
                    new SqlColumn("skinColor", MySqlDbType.Int32),
                    new SqlColumn("eyeColor", MySqlDbType.Int32),
                    new SqlColumn("questsCompleted", MySqlDbType.Int32),
                    new SqlColumn("usingBiomeTorches", MySqlDbType.Int32),
                    new SqlColumn("happyFunTorchTime", MySqlDbType.Int32),
                    new SqlColumn("unlockedBiomeTorches", MySqlDbType.Int32),
                    new SqlColumn("currentLoadoutIndex", MySqlDbType.Int32),
                    new SqlColumn("ateArtisanBread", MySqlDbType.Int32),
                    new SqlColumn("usedAegisCrystal", MySqlDbType.Int32),
                    new SqlColumn("usedAegisFruit", MySqlDbType.Int32),
                    new SqlColumn("usedArcaneCrystal", MySqlDbType.Int32),
                    new SqlColumn("usedGalaxyPearl", MySqlDbType.Int32),
                    new SqlColumn("usedGummyWorm", MySqlDbType.Int32),
                    new SqlColumn("usedAmbrosia", MySqlDbType.Int32),
                    new SqlColumn("unlockedSuperCart", MySqlDbType.Int32),
                    new SqlColumn("enabledSuperCart", MySqlDbType.Int32)
                });
                IQueryBuilder queryBuilder;
                if (database.GetSqlType() != SqlType.Sqlite)
                {
                    queryBuilder = new MysqlQueryCreator();
                }
                else
                {
                    queryBuilder = new SqliteQueryCreator();
                }
                queryBuilder.CreateTable(table);
                SqlTableCreator sqlTableCreator = new SqlTableCreator(database, queryBuilder);
                sqlTableCreator.EnsureTableStructure(table);
            }


            /// <summary>
            /// 从数据库读取一个玩家的备份存档槽，第slot个
            /// </summary>
            /// <param name="player">一个没有什么意义的玩家</param>
            /// <param name="acctid">需要的那个玩家的账号ID</param>
            /// <param name="slot">第几个存档槽</param>
            /// <returns></returns>
            public PlayerData ReadZPlayerDB(TSPlayer player, int acctid, int slot = 1)
            {
                PlayerData playerData = new PlayerData(player);
                playerData.exists = false;
                try
                {
                    using QueryResult queryResult = database.QueryReader("SELECT * FROM " + tableName + " WHERE AccAndSlot=@0", acctid + "-" + slot);
                    if (queryResult.Read())
                    {
                        playerData.exists = true;
                        playerData.health = queryResult.Get<int>("Health");
                        playerData.maxHealth = queryResult.Get<int>("MaxHealth");
                        playerData.mana = queryResult.Get<int>("Mana");
                        playerData.maxMana = queryResult.Get<int>("MaxMana");
                        List<NetItem> list = queryResult.Get<string>("Inventory").Split('~').Select(new Func<string, NetItem>(NetItem.Parse))
                            .ToList();
                        if (list.Count < NetItem.MaxInventory)
                        {
                            list.InsertRange(67, new NetItem[2]);
                            list.InsertRange(77, new NetItem[2]);
                            list.InsertRange(87, new NetItem[2]);
                            list.AddRange(new NetItem[NetItem.MaxInventory - list.Count]);
                        }

                        playerData.inventory = list.ToArray();
                        playerData.extraSlot = queryResult.Get<int>("extraSlot");
                        playerData.spawnX = queryResult.Get<int>("spawnX");
                        playerData.spawnY = queryResult.Get<int>("spawnY");
                        playerData.skinVariant = queryResult.Get<int?>("skinVariant");
                        playerData.hair = queryResult.Get<int?>("hair");
                        playerData.hairDye = (byte)queryResult.Get<int>("hairDye");
                        playerData.hairColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("hairColor"));
                        playerData.pantsColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("pantsColor"));
                        playerData.shirtColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("shirtColor"));
                        playerData.underShirtColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("underShirtColor"));
                        playerData.shoeColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("shoeColor"));
                        playerData.hideVisuals = TShock.Utils.DecodeBoolArray(queryResult.Get<int?>("hideVisuals"));
                        playerData.skinColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("skinColor"));
                        playerData.eyeColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("eyeColor"));
                        playerData.questsCompleted = queryResult.Get<int>("questsCompleted");
                        playerData.usingBiomeTorches = queryResult.Get<int>("usingBiomeTorches");
                        playerData.happyFunTorchTime = queryResult.Get<int>("happyFunTorchTime");
                        playerData.unlockedBiomeTorches = queryResult.Get<int>("unlockedBiomeTorches");
                        playerData.currentLoadoutIndex = queryResult.Get<int>("currentLoadoutIndex");
                        playerData.ateArtisanBread = queryResult.Get<int>("ateArtisanBread");
                        playerData.usedAegisCrystal = queryResult.Get<int>("usedAegisCrystal");
                        playerData.usedAegisFruit = queryResult.Get<int>("usedAegisFruit");
                        playerData.usedArcaneCrystal = queryResult.Get<int>("usedArcaneCrystal");
                        playerData.usedGalaxyPearl = queryResult.Get<int>("usedGalaxyPearl");
                        playerData.usedGummyWorm = queryResult.Get<int>("usedGummyWorm");
                        playerData.usedAmbrosia = queryResult.Get<int>("usedAmbrosia");
                        playerData.unlockedSuperCart = queryResult.Get<int>("unlockedSuperCart");
                        playerData.enabledSuperCart = queryResult.Get<int>("enabledSuperCart");
                    }
                    return playerData;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ReadZPlayerDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ReadZPlayerDB " + ex.ToString());
                    Console.WriteLine("错误：ReadZPlayerDB " + ex.ToString());
                    return playerData;
                }
            }


            /// <summary>
            /// 获取这个用户目前几个备份槽了
            /// </summary>
            /// <param name="player">没意义的玩家</param>
            /// <param name="acctid">需要搜索的该玩家的ID</param>
            /// <param name="text">返回这些槽的name</param>
            /// <returns></returns>
            public int getZPlayerDBMaxSlot(TSPlayer player, int acctid, out List<string> text)
            {
                int num = 0;
                text = new List<string>();
                try
                {
                    using (QueryResult queryResult = database.QueryReader("SELECT * FROM " + tableName + " WHERE Account=@0", new object[]
                    {
                        acctid
                    }))
                    {
                        while (queryResult.Read())
                        {
                            num++;
                            text.Add(queryResult.Get<string>("AccAndSlot"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：getZPlayerDBMaxSlot " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：getZPlayerDBMaxSlot " + ex.ToString());
                    Console.WriteLine("错误：getZPlayerDBMaxSlot " + ex.ToString());
                }
                return num;
            }


            /// <summary>
            /// 将这个玩家的备份数据精准写入到第几个槽
            /// </summary>
            /// <param name="player"></param>
            /// <param name="slot"></param>
            /// <returns></returns>
            public bool WriteZPlayerDB(TSPlayer player, int slot)
            {
                if (!player.IsLoggedIn)
                {
                    return false;
                }
                if (slot > MaxSlot || slot < 1)
                {
                    return false;
                }
                PlayerData playerData = player.PlayerData;
                playerData.CopyCharacter(player);
                //如果没读到，就是不存在，那么写入
                if (!ReadZPlayerDB(player, player.Account.ID, slot).exists)
                {
                    try
                    {
                        database.Query("INSERT INTO " + tableName + " (AccAndSlot, Account, Name, Health, MaxHealth, Mana, MaxMana, Inventory, extraSlot, spawnX, spawnY, skinVariant, hair, hairDye, hairColor, pantsColor, shirtColor, underShirtColor, shoeColor, hideVisuals, skinColor, eyeColor, questsCompleted, usingBiomeTorches, happyFunTorchTime, unlockedBiomeTorches, currentLoadoutIndex,ateArtisanBread, usedAegisCrystal, usedAegisFruit, usedArcaneCrystal, usedGalaxyPearl, usedGummyWorm, usedAmbrosia, unlockedSuperCart, enabledSuperCart) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30, @31, @32, @33, @34, @35);", new object[]
                        {
                                    player.Account.ID.ToString() + "-" + slot.ToString(),
                                    player.Account.ID,
                                    player.Account.Name,
                                    player.TPlayer.statLife,
                                    player.TPlayer.statLifeMax,
                                    player.TPlayer.statMana,
                                    player.TPlayer.statManaMax,
                                    string.Join<NetItem>("~", playerData.inventory),
                                    player.TPlayer.extraAccessory ? 1 : 0,
                                    player.TPlayer.SpawnX,
                                    player.TPlayer.SpawnY,
                                    player.TPlayer.skinVariant,
                                    player.TPlayer.hair,
                                    player.TPlayer.hairDye,
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.hairColor)),
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.pantsColor)),
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.shirtColor)),
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.underShirtColor)),
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.shoeColor)),
                                    TShock.Utils.EncodeBoolArray(player.TPlayer.hideVisibleAccessory),
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.skinColor)),
                                    TShock.Utils.EncodeColor(new Color?(player.TPlayer.eyeColor)),
                                    player.TPlayer.anglerQuestsFinished,
                                    player.TPlayer.UsingBiomeTorches ? 1 : 0,
                                    player.TPlayer.happyFunTorchTime ? 1 : 0,
                                    player.TPlayer.unlockedBiomeTorches ? 1 : 0,
                                    player.TPlayer.CurrentLoadoutIndex,
                                    player.TPlayer.ateArtisanBread ? 1 : 0,
                                    player.TPlayer.usedAegisCrystal ? 1 : 0,
                                    player.TPlayer.usedAegisFruit ? 1 : 0,
                                    player.TPlayer.usedArcaneCrystal ? 1 : 0,
                                    player.TPlayer.usedGalaxyPearl ? 1 : 0,
                                    player.TPlayer.usedGummyWorm ? 1 : 0,
                                    player.TPlayer.usedAmbrosia ? 1 : 0,
                                    player.TPlayer.unlockedSuperCart ? 1 : 0,
                                    player.TPlayer.enabledSuperCart ? 1 : 0
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error("错误：WriteZPlayerDB " + ex.ToString());
                        TSPlayer.All.SendErrorMessage("错误：WriteZPlayerDB " + ex.ToString());
                        Console.WriteLine("错误：WriteZPlayerDB " + ex.ToString());
                        return false;
                    }
                }
                //如果读到了，就是更新
                try
                {
                    database.Query("UPDATE " + tableName + " SET Name = @0, Health = @1, MaxHealth = @2, Mana = @3, MaxMana = @4, Inventory = @5, Account = @6, spawnX = @7, spawnY = @8, hair = @9, hairDye = @10, hairColor = @11, pantsColor = @12, shirtColor = @13, underShirtColor = @14, shoeColor = @15, hideVisuals = @16, skinColor = @17, eyeColor = @18, questsCompleted = @19, skinVariant = @20, extraSlot = @21, usingBiomeTorches = @22, happyFunTorchTime = @23, unlockedBiomeTorches = @24, currentLoadoutIndex = @25, ateArtisanBread = @26, usedAegisCrystal = @27, usedAegisFruit = @28, usedArcaneCrystal = @29, usedGalaxyPearl = @30, usedGummyWorm = @31, usedAmbrosia = @32, unlockedSuperCart = @33, enabledSuperCart = @34 WHERE AccAndSlot = @35;", new object[]
                    {
                                player.Account.Name,
                                player.TPlayer.statLife,
                                player.TPlayer.statLifeMax,
                                player.TPlayer.statMana,
                                player.TPlayer.statManaMax,
                                string.Join<NetItem>("~", playerData.inventory),
                                player.Account.ID,
                                player.TPlayer.SpawnX,
                                player.TPlayer.SpawnY,
                                player.TPlayer.hair,
                                player.TPlayer.hairDye,
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.hairColor)),
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.pantsColor)),
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.shirtColor)),
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.underShirtColor)),
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.shoeColor)),
                                TShock.Utils.EncodeBoolArray(player.TPlayer.hideVisibleAccessory),
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.skinColor)),
                                TShock.Utils.EncodeColor(new Color?(player.TPlayer.eyeColor)),
                                player.TPlayer.anglerQuestsFinished,
                                player.TPlayer.skinVariant,
                                player.TPlayer.extraAccessory ? 1 : 0,
                                player.TPlayer.UsingBiomeTorches ? 1 : 0,
                                player.TPlayer.happyFunTorchTime ? 1 : 0,
                                player.TPlayer.unlockedBiomeTorches ? 1 : 0,
                                player.TPlayer.CurrentLoadoutIndex,
                                player.TPlayer.ateArtisanBread ? 1 : 0,
                                player.TPlayer.usedAegisCrystal ? 1 : 0,
                                player.TPlayer.usedAegisFruit ? 1 : 0,
                                player.TPlayer.usedArcaneCrystal ? 1 : 0,
                                player.TPlayer.usedGalaxyPearl ? 1 : 0,
                                player.TPlayer.usedGummyWorm ? 1 : 0,
                                player.TPlayer.usedAmbrosia ? 1 : 0,
                                player.TPlayer.unlockedSuperCart ? 1 : 0,
                                player.TPlayer.enabledSuperCart ? 1 : 0,
                                player.Account.ID.ToString() + "-" + slot.ToString()
                    });
                    return true;
                }
                catch (Exception ex2)
                {
                    TShock.Log.Error("错误：WriteZPlayerDB 2 " + ex2.ToString());
                    TSPlayer.All.SendErrorMessage("错误：WriteZPlayerDB 2 " + ex2.ToString());
                    Console.WriteLine("错误：WriteZPlayerDB 2 " + ex2.ToString());
                    return false;
                }
            }


            /// <summary>
            /// 添加一个用户的备份槽，自动将存档槽像后排，排到大于5自动删除
            /// </summary>
            /// <param name="player"></param>
            /// <returns></returns>
            public bool AddZPlayerDB(TSPlayer player)
            {
                if (!player.IsLoggedIn)
                {
                    return false;
                }
                int num = getZPlayerDBMaxSlot(player, player.Account.ID, out List<string> text);
                if (num < 5)
                {
                    try
                    {
                        for (int i = num + 1; i > 1; i--)
                        {
                            database.Query("UPDATE " + tableName + " SET AccAndSlot = @0 WHERE AccAndSlot = @1;", new object[]
                            {
                                    player.Account.ID.ToString() + "-" + i.ToString(),
                                    player.Account.ID.ToString() + "-" + (i - 1).ToString()
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error("错误：AddZPlayerDB " + ex.ToString());
                        TSPlayer.All.SendErrorMessage("错误：AddZPlayerDB " + ex.ToString());
                        Console.WriteLine("错误：AddZPlayerDB " + ex.ToString());
                        return false;
                    }
                    return WriteZPlayerDB(player, 1);
                }
                else
                {
                    try
                    {
                        text.RemoveAll((string x) =>
                        x.Equals(player.Account.ID.ToString() + "-" + 1.ToString()) ||
                        x.Equals(player.Account.ID.ToString() + "-" + 2.ToString()) ||
                        x.Equals(player.Account.ID.ToString() + "-" + 3.ToString()) ||
                        x.Equals(player.Account.ID.ToString() + "-" + 4.ToString())
                        );
                        foreach (string str in text)
                        {
                            database.Query("DELETE FROM " + tableName + " WHERE AccAndSlot = @0;", new object[]
                            { str });
                        }
                    }
                    catch (Exception ex2)
                    {
                        TShock.Log.Error("错误：AddZPlayerDB 1 " + ex2.ToString());
                        TSPlayer.All.SendErrorMessage("错误：AddZPlayerDB 1 " + ex2.ToString());
                        Console.WriteLine("错误：AddZPlayerDB 1 " + ex2.ToString());
                        return false;
                    }
                    return AddZPlayerDB(player);
                }
            }


            /// <summary>
            /// 清理所有人的备份存档
            /// </summary>
            /// <param name="zdb"></param>
            /// <returns></returns>
            public bool ClearALLZPlayerDB(ref ZplayerDB zdb)
            {
                try
                {
                    database.Query("DROP TABLE " + tableName, Array.Empty<object>());
                    zdb = new ZplayerDB(TShock.DB);
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ClearALLZPlayerDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ClearALLZPlayerDB " + ex.ToString());
                    Console.WriteLine("错误：ClearALLZPlayerDB " + ex.ToString());
                    return false;
                }
            }


            /// <summary>
            /// 清理该用户的备份存档
            /// </summary>
            /// <param name="account"></param>
            /// <returns></returns>
            public bool ClearZPlayerDB(int account)
            {
                try
                {
                    database.Query("DELETE FROM " + tableName + " WHERE Account = @0;", new object[]
                    {
                        account
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ClearZPlayerDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ClearZPlayerDB " + ex.ToString());
                    Console.WriteLine("错误：ClearZPlayerDB " + ex.ToString());
                    return false;
                }
            }
        }


        /// <summary>
        /// 额外数据库类
        /// </summary>
        public class ZplayerExtraDB
        {
            private IDbConnection database;
            private string tableName;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="db"></param>
            public ZplayerExtraDB(IDbConnection db)
            {
                database = db;
                tableName = "Zhipm_PlayerExtra";
                SqlTable table = new SqlTable(tableName, new SqlColumn[]
                {
                    new SqlColumn("Account", MySqlDbType.Int32)
                    {
                        Primary = true
                    },
                    new SqlColumn("Name", MySqlDbType.Text),
                    new SqlColumn("time", MySqlDbType.Int64)
                });
                IQueryBuilder queryBuilder;
                if (database.GetSqlType() != SqlType.Sqlite)
                {
                    queryBuilder = new MysqlQueryCreator();
                }
                else
                {
                    queryBuilder = new SqliteQueryCreator();
                }
                queryBuilder.CreateTable(table);
                SqlTableCreator sqlTableCreator = new SqlTableCreator(database, queryBuilder);
                sqlTableCreator.EnsureTableStructure(table);
            }


            /// <summary>
            /// 从数据库中读取一个用户的额外数据库数据
            /// </summary>
            /// <param name="account">账户id,不是游戏内索引</param>
            /// <returns></returns>
            public ExtraData? ReadExtraDB(int account)
            {
                ExtraData? extraData = null;
                try
                {
                    using (QueryResult queryResult = database.QueryReader("SELECT * FROM " + tableName + " WHERE Account=@0", new object[]
                    {
                        account
                    }))
                    {
                        if (queryResult.Read())
                        {
                            extraData = new ExtraData();
                            extraData.Account = account;
                            extraData.Name = queryResult.Get<string>("Name");
                            extraData.time = queryResult.Get<long>("time");
                        }
                    }
                    return extraData;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ReadExtraDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ReadExtraDB " + ex.ToString());
                    Console.WriteLine("错误：ReadExtraDB " + ex.ToString());
                    return null;
                }
            }


            /// <summary>
            /// 获取这个玩家游戏的时间，从数据库里获取
            /// </summary>
            /// <param name="account"></param>
            /// <returns></returns>
            public long getPlayerExtraDBTime(int account)
            {
                ExtraData? extraData = ReadExtraDB(account);
                if (extraData == null)
                {
                    return 0L;
                }
                else
                {
                    return extraData.time;
                }
            }


            /// <summary>
            /// 将这个用户的额外数据写入数据库
            /// </summary>
            /// <param name="account"></param>
            /// <param name="ed"></param>
            /// <returns></returns>
            public bool WriteExtraDB(ExtraData ed)
            {
                //如果数据库中没有这个数据，那么新增一个
                if (ReadExtraDB(ed.Account) == null)
                {
                    try
                    {
                        database.Query("INSERT INTO " + tableName + " (Account, Name, time) VALUES (@0, @1, @2);", new object[]
                        {
                                ed.Account,
                                ed.Name,
                                ed.time
                        });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error("错误：WriteExtraDB " + ex.ToString());
                        TSPlayer.All.SendErrorMessage("错误：WriteExtraDB " + ex.ToString());
                        Console.WriteLine("错误：WriteExtraDB " + ex.ToString());
                        return false;
                    }
                }
                //否则就更新数据
                try
                {
                    database.Query("UPDATE " + tableName + " SET Name = @0, time = @1 WHERE Account = @2;", new object[]
                    {
                            ed.Name,
                            ed.time,
                            ed.Account
                    });
                    return true;
                }
                catch (Exception ex2)
                {
                    TShock.Log.Error("错误：WriteExtraDB 2 " + ex2.ToString());
                    TSPlayer.All.SendErrorMessage("错误：WriteExtraDB 2 " + ex2.ToString());
                    Console.WriteLine("错误：WriteExtraDB 2 " + ex2.ToString());
                    return false;
                }
            }


            /// <summary>
            /// 清理所有用户的额外数据库，从数据库里删除
            /// </summary>
            /// <param name="zedb"></param>
            /// <returns></returns>
            public bool ClearALLZPlayerExtraDB(ref ZplayerExtraDB zedb)
            {
                try
                {
                    database.Query("DROP TABLE " + tableName, Array.Empty<object>());
                    zedb = new ZplayerExtraDB(TShock.DB);
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ClearALLZPlayerExtraDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ClearALLZPlayerExtraDB " + ex.ToString());
                    Console.WriteLine("错误：ClearALLZPlayerExtraDB " + ex.ToString());
                    return false;
                }
            }


            /// <summary>
            /// 清理某个账户的额外数据库，从数据库里删除
            /// </summary>
            /// <param name="account"></param>
            /// <returns></returns>
            public bool ClearZPlayerExtraDB(int account)
            {
                try
                {
                    database.Query("DELETE FROM " + tableName + " WHERE Account = @0;", new object[]
                    {
                        account
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ClearZPlayerExtraDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ClearZPlayerExtraDB " + ex.ToString());
                    Console.WriteLine("错误：ClearZPlayerExtraDB " + ex.ToString());
                    return false;
                }
            }


            /// <summary>
            /// 获取当前额外数据库的所有成员
            /// </summary>
            /// <returns></returns>
            public List<ExtraData> ListAllExtraDB()
            {
                List<ExtraData> list = new List<ExtraData>();
                try
                {
                    using (QueryResult queryResult = database.QueryReader("SELECT * FROM " + tableName))
                    {
                        while (queryResult.Read())
                        {
                            list.Add(new ExtraData
                            {
                                Account = queryResult.Get<int>("Account"),
                                Name = queryResult.Get<string>("Name"),
                                time = queryResult.Get<long>("time")
                            });
                        }
                    }
                    return list;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("错误：ListAllExtraDB " + ex.ToString());
                    TSPlayer.All.SendErrorMessage("错误：ListAllExtraDB " + ex.ToString());
                    Console.WriteLine("错误：ListAllExtraDB " + ex.ToString());
                    return list;
                }
            }
        }


        /// <summary>
        /// 额外数据库的类
        /// </summary>
        public class ExtraData
        {
            public int Account;
            public string Name;
            public long time;
            public ExtraData()
            { }
            public ExtraData(int Account, string Name, long time)
            {
                this.Account = Account;
                this.Name = Name;
                this.time = time;
            }
        }
    }
}
