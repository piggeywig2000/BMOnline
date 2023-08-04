using System;
using UnhollowerBaseLib.Runtime;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Flash2;
using BMOnline.Common.Gamemodes;

namespace BMOnline.Mod.Patches
{
    internal static class TextManagerPatch
    {
        private delegate IntPtr GetTextDelegate(IntPtr _thisPtr, SystemLanguage language, IntPtr key);
        private static GetTextDelegate GetTextInstance;
        private static GetTextDelegate GetTextOriginal;

        public static unsafe void CreateDetour()
        {
            GetTextInstance = GetText;
            GetTextOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(Framework.Text.TextManager).GetMethod(nameof(Framework.Text.TextManager.getText)))
                .GetValue(null)).MethodPointer, GetTextInstance);
        }

        static IntPtr GetText(IntPtr _thisPtr, SystemLanguage language, IntPtr key)
        {
            string keyStr = IL2CPP.Il2CppStringToManaged(key);
            if (!string.IsNullOrEmpty(keyStr) && keyStr == "maingame_practice_mode")
            {
                if (MainGame.gameKind == (MainGameDef.eGameKind)OnlineGamemode.RaceMode)
                    key = IL2CPP.ManagedStringToIl2Cpp("maingame_onlineracemode");
                else if (MainGame.gameKind == (MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode)
                    key = IL2CPP.ManagedStringToIl2Cpp("maingame_onlinetimeattackmode");
            }
            return GetTextOriginal(_thisPtr, language, key);
        }
    }
}
