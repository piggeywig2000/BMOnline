using System.Collections.Generic;
using BMOnline.Common;
using BMOnline.Mod.Patches;
using Flash2;

namespace BMOnline.Mod
{
    public static class Main
    {
        private static BMOnlineApi api;
        public static IBMOnlineApi Api { get => api; }

        public static void OnModLoad(Dictionary<string, object> settingsDict)
        {
            Log.Info("Loading online multiplayer mod");
            api = new BMOnlineApi(settingsDict);
        }

        public static void OnModFixedUpdate()
        {
            api.FixedUpdate();
        }

        public static void OnModUpdate()
        {
            if (api != null && !api.IsInitialised && !api.IsFatalErrored)
            {
                Log.Info("Patching methods");
                MainGameStagePatch.CreateDetour();
                PlayerMotionPatch.CreateDetour();
                AppInputPatch.CreateDetour();

                api.Initialise(SteamManager.GetFriendsHandler().GetPersonaName());
            }

            api.Update();
        }

        public static void OnModLateUpdate()
        {
            api.LateUpdate();
        }
    }
}
