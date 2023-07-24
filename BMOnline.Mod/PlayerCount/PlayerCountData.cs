using Flash2;

namespace BMOnline.Mod.PlayerCount
{
    internal readonly struct PlayerCountData
    {
        public readonly PlayerCountDictionary<SelectorDef.MainGameKind> ModeCounts;
        public readonly PlayerCountDictionary<MainGameDef.eCourse> CourseCounts;
        public readonly PlayerCountDictionary<ushort> StageCounts;

        public PlayerCountData()
        {
            ModeCounts = new PlayerCountDictionary<SelectorDef.MainGameKind>();
            CourseCounts = new PlayerCountDictionary<MainGameDef.eCourse>();
            StageCounts = new PlayerCountDictionary<ushort>();
        }
    }
}
