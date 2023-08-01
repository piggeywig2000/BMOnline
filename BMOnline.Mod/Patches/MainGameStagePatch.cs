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

        private delegate void FixedUpdateDelegate(IntPtr _thisPtr);
        private static FixedUpdateDelegate FixedUpdateInstance;
        private static FixedUpdateDelegate FixedUpdateOriginal;

        public static MainGameStage MainGameStage { get; private set; } = null;
        public static event EventHandler OnReset;

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

            FixedUpdateInstance = FixedUpdate;
            FixedUpdateOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.FixedUpdate)))
                .GetValue(null)).MethodPointer, FixedUpdateInstance);
        }

        static void Initialize(IntPtr _thisPtr, int index, MainGameDef.eGameKind in_gameKind, IntPtr in_mgStageDatum, IntPtr in_mgBgDatum, int playerIndex)
        {
            MainGameStage __instance = new MainGameStage(_thisPtr);
            MgStageDatum mgStageDatum = new MgStageDatum(in_mgStageDatum);
            MgBgDatum mgBgDatum = new MgBgDatum(in_mgBgDatum);

            MainGameStage = __instance;

            InitializeOriginal(_thisPtr, index, in_gameKind, in_mgStageDatum, in_mgBgDatum, playerIndex);
        }

        static void OnDestroy(IntPtr _thisPtr)
        {
            MainGameStage __instance = new MainGameStage(_thisPtr);

            if (MainGameStage == __instance)
            {
                MainGameStage = null;
            }

            OnDestroyOriginal(_thisPtr);
        }

        static void ResetGameObject(IntPtr _thisPtr, bool isResetBanana)
        {
            MainGameStage __instance = new MainGameStage(_thisPtr);

            if (MainGameStage == __instance)
            {
                OnReset?.Invoke(null, EventArgs.Empty);
            }

            ResetGameObjectOriginal(_thisPtr, isResetBanana);
        }

        static void FixedUpdate(IntPtr _thisPtr)
        {
            FixedUpdateOriginal(_thisPtr);
            MainGameStage mainGameStage = new MainGameStage(_thisPtr);
            if (mainGameStage.state == MainGameStage.State.GAME && mainGameStage.gameKind == (MainGameDef.eGameKind)9 && AppInput.State(mainGameStage.m_PlayerIndex).ButtonDown(AppInput.eAction.MainGame_QuickRetry))
            {
                mainGameStage.m_State = MainGameStage.State.RETRY;
                mainGameStage.m_StateFrame = 0;
                mainGameStage.m_StateTimer = 0;
                mainGameStage.m_StepSec.isActive = false;
                mainGameStage.m_isPausable = false;
            }
        }
    }
}
