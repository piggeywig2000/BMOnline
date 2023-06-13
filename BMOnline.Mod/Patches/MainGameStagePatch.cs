using System;
using Flash2;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;

namespace BMOnline.Mod.Patches
{
    internal static class MainGameStagePatch
    {
        private delegate void InitializeDelegate(IntPtr _thisPtr, int index, MainGameDef.eGameKind in_gameKind, IntPtr in_mgStageDatum, IntPtr in_mgBgDatum, int playerIndex);
        private static InitializeDelegate InitializeInstance;
        private static InitializeDelegate InitializeOriginal;

        private delegate void OnDestroyDelegate(IntPtr _thisPtr);
        private static OnDestroyDelegate OnDestroyInstance;
        private static OnDestroyDelegate OnDestroyOriginal;

        private delegate void ResetGameObjectDelegate(IntPtr _thisPtr, bool isResetBanana);
        private static ResetGameObjectDelegate ResetGameObjectInstance;
        private static ResetGameObjectDelegate ResetGameObjectOriginal;

        public static unsafe void CreateDetour()
        {
            InitializeInstance = Initialize;
            InitializeOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.Initialize)))
                .GetValue(null)).MethodPointer, InitializeInstance);

            OnDestroyInstance = OnDestroy;
            OnDestroyOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.OnDestroy)))
                .GetValue(null)).MethodPointer, OnDestroyInstance);

            ResetGameObjectInstance = ResetGameObject;
            ResetGameObjectOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.ResetGameObject)))
                .GetValue(null)).MethodPointer, ResetGameObjectInstance);

        }

        static void Initialize(IntPtr _thisPtr, int index, MainGameDef.eGameKind in_gameKind, IntPtr in_mgStageDatum, IntPtr in_mgBgDatum, int playerIndex)
        {
            MainGameStage __instance = new MainGameStage(_thisPtr);
            MgStageDatum mgStageDatum = new MgStageDatum(in_mgStageDatum);
            MgBgDatum mgBgDatum = new MgBgDatum(in_mgBgDatum);

            Main.mainGameStage = __instance;
            Main.wasMainStageCreated = true;

            InitializeOriginal(_thisPtr, index, in_gameKind, in_mgStageDatum, in_mgBgDatum, playerIndex);
        }

        static void OnDestroy(IntPtr _thisPtr)
        {
            MainGameStage __instance = new MainGameStage(_thisPtr);

            if (Main.mainGameStage == __instance)
            {
                Main.mainGameStage = null;
                Main.wasMainStageDestroyed = true;
            }

            OnDestroyOriginal(_thisPtr);
        }

        static void ResetGameObject(IntPtr _thisPtr, bool isResetBanana)
        {
            MainGameStage __instance = new MainGameStage(_thisPtr);

            if (Main.mainGameStage == __instance)
            {
                Main.wasMainStageReset = true;
            }

            ResetGameObjectOriginal(_thisPtr, isResetBanana);
        }
    }
}
