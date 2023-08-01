using System;
using Flash2;
using UnhollowerBaseLib.Runtime;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace BMOnline.Mod.Patches
{
    internal static class GameParamPatch
    {
        private delegate void ResetParamDelegate(IntPtr _thisPtr, bool isOnBoot);
        private static ResetParamDelegate ResetParamInstance;
        private static ResetParamDelegate ResetParamOriginal;

        public static unsafe void CreateDetour()
        {
            ResetParamInstance = ResetParam;
            ResetParamOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(GameParam).GetMethod(nameof(GameParam.ResetParam)))
                .GetValue(null)).MethodPointer, ResetParamInstance);
        }

        static void ResetParam(IntPtr _thisPtr, bool isOnBoot)
        {
            ResetParamOriginal(_thisPtr, isOnBoot);
            GameParam gameParam = new GameParam(_thisPtr);
            if (gameParam.m_SkipParam.m_SkipMgHowToPlayArray.Length <= 8)
            {
                bool[] skipHowToPlayArray = new bool[9];
                Array.Copy(gameParam.m_SkipParam.m_SkipMgHowToPlayArray, skipHowToPlayArray, 8);
                skipHowToPlayArray[8] = false;
                gameParam.m_SkipParam.m_SkipMgHowToPlayArray = skipHowToPlayArray;
            }
        }
    }
}
