using System;
using Flash2;

namespace BMOnline.Mod.Players
{
    public interface IOnlinePlayer
    {
        public ushort Id { get; }
        public string Name { get; }
        public MainGameDef.eGameKind Mode { get; }
        public MainGameDef.eCourse Course { get; }
        public ushort? Stage { get; }
        public Chara.SelectDatum SelectedCharacter { get; }
        public CharaCustomize.PartsSet Customisations { get; }
        public IPlayerGameInfo GameInfo { get; }

        /// <summary>
        /// Raised when the player joins the current stage.
        /// </summary>
        public event EventHandler OnPlayerLoaded;

        /// <summary>
        /// Raised when the player leaves the current stage.
        /// </summary>
        public event EventHandler OnPlayerUnloaded;

        /// <summary>
        /// Raised when the player information is changed, which happens when the player enters/exits a stage.
        /// </summary>
        public event EventHandler OnPlayerUpdated;
    }
}
