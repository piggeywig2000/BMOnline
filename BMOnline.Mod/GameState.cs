using BMOnline.Common;
using BMOnline.Mod.Patches;
using Flash2;

namespace BMOnline.Mod
{
    internal class GameState
    {
        public GameState()
        {
            MainGameStagePatch.OnReset += (s, e) => WasMainStageReset = true;
        }

        public MainGameStage MainGameStage => MainGameStagePatch.MainGameStage;
        public bool WasMainStageReset { get; private set; } = false;
        public bool IsInGame { get => MainGameStage != null && !MainGameStage.m_IsFullReplay; }

        public void ClearFlags()
        {
            WasMainStageReset = false;
        }
    }
}
