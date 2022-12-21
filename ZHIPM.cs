using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ZHIPlayerManager
{
    [ApiVersion(2, 1)]
    public partial class ZHIPM : TerrariaPlugin
    {
        public override string Author => "z枳";

        public override string Description => "玩家管理，提供修改玩家的任何信息，允许玩家备份，可以回档等操作";

        public override string Name => "ZHIPlayerManager";

        public override Version Version => new Version(1, 0, 0, 0);

        //人物备份数据库
        public ZplayerDB ZPDataBase;
        //额外数据库
        public ZplayerExtraDB ZPExtraDB;
        //用来同步额外数据库的集合
        public List<ExtraData> edPlayers;
        //广播颜色
        public Color broadcastColor = new Color(0, 255, 213);
        //计时器，60 Timer = 1 秒
        public static long Timer;

        public ZHIPM(Main game) : base(game) { }

        public override void Initialize()
        {
            Timer = 0L;
            ZPDataBase = new ZplayerDB(TShock.DB);
            ZPExtraDB = new ZplayerExtraDB(TShock.DB);
            edPlayers = new List<ExtraData>();

            //用来对玩家进行额外数据库更新
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            //限制玩家名字类型
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            //同步玩家的额外数据库
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);


            #region 指令
            Commands.ChatCommands.Add(new Command("", Help, "zhelp")
            {
                HelpText = "输入 /zhelp  来查看指令帮助"
            });
            Commands.ChatCommands.Add(new Command("zhipm.save", MySSCSave, "zsave")
            {
                HelpText = "输入 /zsave  来备份自己的人物存档"
            });
            Commands.ChatCommands.Add(new Command("zhipm.save", MySSCSaveAuto, "zsaveauto")
            {
                HelpText = "输入 /zsaveauto [minute]  来每隔 minute 分钟自动备份自己的人物存档，当 minute 为 0 时关闭该功能"
            });
            Commands.ChatCommands.Add(new Command("zhipm.save", ViewMySSCSave, "zvisa")
            {
                HelpText = "输入 /zvisa [num] 来查看自己的人物备份"
            });
            Commands.ChatCommands.Add(new Command("zhipm.back", MySSCBack, "zback")
            {
                HelpText = "输入 /zback [name]  来读取该玩家的人物存档\n输入 /zback [name] [num]  来读取该玩家的第几个人物存档"
            });
            Commands.ChatCommands.Add(new Command("zhipm.clone", SSCClone, "zclone")
            {
                HelpText = "输入 /zclone [name1] [name2]  将玩家1的人物数据复制给玩家2\n输入 /zclone [name]  将该玩家的人物数据复制给自己"
            });
            Commands.ChatCommands.Add(new Command("zhipm.modify", SSCModify, "zmodify")
            {
                HelpText = "输入 /zmodify help  查看修改玩家数据的指令帮助"
            });
            Commands.ChatCommands.Add(new Command("zhipm.export", ZhiExportPlayer, "zout")
            {
                HelpText = "输入 /zout [name]  来导出该玩家的人物存档\n输入 /zout all  来导出所有人物的存档"
            });
            Commands.ChatCommands.Add(new Command("zhipm.sort", ZhiSortPlayer, "zsort")
            {
                HelpText = "输入 /zsort help  来查看排序系列指令帮助"
            });


            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayerDB, "zresetdb")
            {
                HelpText = "输入 /zresetdb [name]  来清理该玩家的备份数据\n输入 /zresetdb all  来清理所有玩家的备份数据"
            });
            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayerEX, "zresetex")
            {
                HelpText = "输入 /zresetex [name]  来清理该玩家的额外数据\n输入 /zresetex all  来清理所有玩家的额外数据"
            });
            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayer, "zreset")
            {
                HelpText = "输入 /zreset [name]  来清理该玩家的人物数据\n输入 /zreset all  来清理所有玩家的人物数据"
            });
            Commands.ChatCommands.Add(new Command("zhipm.reset", ZResetPlayerAll, "zresetallplayers")
            {
                HelpText = "输入 /zresetallplayers  来清理所有玩家的所有人物数据"
            });


            Commands.ChatCommands.Add(new Command("zhipm.vi", ViewInvent, "vi")
            {
                HelpText = "输入 /vi [name]  来查看该玩家的库存"
            });
            Commands.ChatCommands.Add(new Command("zhipm.vi", ViewInventDisorder, "vid")
            {
                HelpText = "输入 /vid [name]  来查看该玩家的库存，不分类"
            });
            Commands.ChatCommands.Add(new Command("zhipm.vi", ViewInventText, "vit")
            {
                HelpText = "输入 /vit [name]  来查看该玩家的库存，不分类仅文本"
            });
            Commands.ChatCommands.Add(new Command("zhipm.vs", ViewState, "vs")
            {
                HelpText = "输入 /vs [name]  来查看该玩家的状态"
            });
            Commands.ChatCommands.Add(new Command("zhipm.vs", ViewStateText, "vst")
            {
                HelpText = "输入 /vst [name]  来查看该玩家的状态，仅文本"
            });


            Commands.ChatCommands.Add(new Command("zhipm.ban", SuperBan, "zban")
            {
                HelpText = "输入 /zban add [name]  来封禁无论是否在线的玩家"
            });
            #endregion
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }
            base.Dispose(disposing);
        }
    }
}
