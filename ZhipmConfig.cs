using Newtonsoft.Json;
using TShockAPI;

namespace ZHIPlayerManager
{
    public class ZhipmConfig
    {
        static string configPath = Path.Combine(TShock.SavePath + "/Zhipm", "ZhiPlayerManager.json");

        /// <summary>
        /// 从文件中导出
        /// </summary>
        /// <returns></returns>
        public static ZhipmConfig LoadConfigFile()
        {
            if (!Directory.Exists(TShock.SavePath + "/Zhipm"))
            {
                Directory.CreateDirectory(TShock.SavePath + "/Zhipm");
            }
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new ZhipmConfig(
                    true, true, true, false, true, false, true, true, 20, 5, new List<int>()
                    ), Formatting.Indented));
            }

            return JsonConvert.DeserializeObject<ZhipmConfig>(File.ReadAllText(configPath));
        }

        public ZhipmConfig() { }

        public ZhipmConfig(bool 是否启用在线时长统计, bool 是否启用死亡次数统计, bool 是否启用击杀NPC统计, bool 是否启用点数统计, bool 默认击杀字体是否对玩家显示, bool 默认点数字体是否对玩家显示, bool 是否启用击杀Boss伤害排行榜, bool 是否启用玩家自动备份, int 默认自动备份的时间_单位分钟_若为0代表关闭, int 每个玩家最多几个备份存档, List<int> 哪些生物也包含进击杀伤害排行榜)
        {
            this.是否启用在线时长统计 = 是否启用在线时长统计;
            this.是否启用死亡次数统计 = 是否启用死亡次数统计;
            this.是否启用击杀NPC统计 = 是否启用击杀NPC统计;
            this.是否启用点数统计 = 是否启用点数统计;
            this.默认击杀字体是否对玩家显示 = 默认击杀字体是否对玩家显示;
            this.默认点数字体是否对玩家显示 = 默认点数字体是否对玩家显示;
            this.是否启用击杀Boss伤害排行榜 = 是否启用击杀Boss伤害排行榜;
            this.是否启用玩家自动备份 = 是否启用玩家自动备份;
            this.默认自动备份的时间_单位分钟_若为0代表关闭 = 默认自动备份的时间_单位分钟_若为0代表关闭;
            this.每个玩家最多几个备份存档 = 每个玩家最多几个备份存档;
            this.哪些生物也包含进击杀伤害排行榜 = 哪些生物也包含进击杀伤害排行榜;
        }

        public bool 是否启用在线时长统计;
        public bool 是否启用死亡次数统计;
        public bool 是否启用击杀NPC统计;
        public bool 是否启用点数统计;
        public bool 默认击杀字体是否对玩家显示;
        public bool 默认点数字体是否对玩家显示;
        public bool 是否启用击杀Boss伤害排行榜;
        public bool 是否启用玩家自动备份;
        public int 默认自动备份的时间_单位分钟_若为0代表关闭;
        public int 每个玩家最多几个备份存档;
        public List<int> 哪些生物也包含进击杀伤害排行榜;
    }
}
