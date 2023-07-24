using System.Collections.Generic;
using System.Collections.ObjectModel;
using BMOnline.Mod.Players;
using Flash2;

namespace BMOnline.Mod.PlayerCount
{
    internal readonly struct PlayerCountData
    {
        public readonly PlayerCountDictionary<SelectorDef.MainGameKind> ModeCounts;
        public readonly PlayerCountDictionary<MainGameDef.eCourse> CourseCounts;
        public readonly PlayerCountDictionary<ushort> StageCounts;
        public readonly ReadOnlyDictionary<SelectorDef.MainGameKind, List<IOnlinePlayer>> ModeToPlayers;

        public PlayerCountData()
        {
            ModeCounts = new PlayerCountDictionary<SelectorDef.MainGameKind>();
            CourseCounts = new PlayerCountDictionary<MainGameDef.eCourse>();
            StageCounts = new PlayerCountDictionary<ushort>();
            ModeToPlayers = new ReadOnlyDictionary<SelectorDef.MainGameKind, List<IOnlinePlayer>>(new Dictionary<SelectorDef.MainGameKind, List<IOnlinePlayer>>()
            {
                { SelectorDef.MainGameKind.Story, new List<IOnlinePlayer>() },
                { SelectorDef.MainGameKind.Challenge_SMB1, new List<IOnlinePlayer>() },
                { SelectorDef.MainGameKind.Challenge_SMB2, new List<IOnlinePlayer>() },
                { SelectorDef.MainGameKind.Practice_SMB1, new List<IOnlinePlayer>() },
                { SelectorDef.MainGameKind.Practice_SMB2, new List<IOnlinePlayer>() },
                { SelectorDef.MainGameKind.Special, new List<IOnlinePlayer>() },
                { SelectorDef.MainGameKind.TimeAttack, new List<IOnlinePlayer>() }
            });
        }
    }
}
