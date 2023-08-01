using System;
using Flash2;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace BMOnline.Mod.Patches
{
    internal static class MainGameStagePatch
    {
        public static RaceManager RaceManager { private get; set; }

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

        private delegate void MdActivatePlayerDelegate(IntPtr _thisPtr);
        private static MdActivatePlayerDelegate MdActivatePlayerInstance;
        private static MdActivatePlayerDelegate MdActivatePlayerOriginal;

        private delegate void UpdateTimeupDelegate(IntPtr _thisPtr);
        private static UpdateTimeupDelegate UpdateTimeupInstance;
        private static UpdateTimeupDelegate UpdateTimeupOriginal;

        private delegate void UpdateFallOutDelegate(IntPtr _thisPtr);
        private static UpdateFallOutDelegate UpdateFallOutInstance;
        private static UpdateFallOutDelegate UpdateFallOutOriginal;

        private delegate void UpdateGoalSubEffectPlus1Delegate(IntPtr _thisPtr);
        private static UpdateGoalSubEffectPlus1Delegate UpdateGoalSubEffectPlus1Instance;
        private static UpdateGoalSubEffectPlus1Delegate UpdateGoalSubEffectPlus1Original;

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

            MdActivatePlayerInstance = MdActivatePlayer;
            MdActivatePlayerOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage.ReadyGoSequenceNormal).GetMethod(nameof(MainGameStage.ReadyGoSequenceNormal.mdActivatePlayer)))
                .GetValue(null)).MethodPointer, MdActivatePlayerInstance);

            UpdateTimeupInstance = UpdateTimeup;
            UpdateTimeupOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.updateTimeup)))
                .GetValue(null)).MethodPointer, UpdateTimeupInstance);

            UpdateFallOutInstance = UpdateFallOut;
            UpdateFallOutOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.updateFallOut)))
                .GetValue(null)).MethodPointer, UpdateFallOutInstance);

            UpdateGoalSubEffectPlus1Instance = UpdateGoalSubEffectPlus1;
            UpdateGoalSubEffectPlus1Original = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage).GetMethod(nameof(MainGameStage.updateGoalSub_EFFECT_Plus1)))
                .GetValue(null)).MethodPointer, UpdateGoalSubEffectPlus1Instance);
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

        static void MdActivatePlayer(IntPtr _thisPtr)
        {
            if (MainGame.mainGameStage.gameKind == (MainGameDef.eGameKind)9 && !Input.GetKey(KeyCode.G))
            {
                RaceManager.ChangeLoading(true);
                return;
            }
            RaceManager.ChangeLoading(false);
            MdActivatePlayerOriginal(_thisPtr);
        }

        static void UpdateTimeup(IntPtr _thisPtr)
        {
            MainGameStage mainGameStage = new MainGameStage(_thisPtr);
            if (mainGameStage.gameKind == (MainGameDef.eGameKind)9 && mainGameStage.m_SubState == 1 && mainGameStage.m_StateFrame > Util.SecToFrame(MainGameStage.sTimeupWait))
            {
                mainGameStage.m_SubState = 3;
                MainGame.Instance.FadeOut();
            }
            UpdateTimeupOriginal(_thisPtr);
        }

        static void UpdateFallOut(IntPtr _thisPtr)
        {
            MainGameStage mainGameStage = new MainGameStage(_thisPtr);
            if (mainGameStage.gameKind == (MainGameDef.eGameKind)9 && mainGameStage.m_SubState == 1 && mainGameStage.m_StateFrame > Util.SecToFrame(MainGame.Instance.m_ReplayParam.FalloutPreTime))
            {
                mainGameStage.m_SubState = 5;
                MainGame.Instance.FadeOut();
            }
            UpdateFallOutOriginal(_thisPtr);
        }

        static void UpdateGoalSubEffectPlus1(IntPtr _thisPtr)
        {
            MainGameStage mainGameStage = new MainGameStage(_thisPtr);
            if (mainGameStage.gameKind == (MainGameDef.eGameKind)9 && mainGameStage.m_SubState == 4 && mainGameStage.m_StateFrame >= 185f)
            {
                mainGameStage.m_SelectedResultButton = MgResultMenu.eTextKind.Retry;
                mainGameStage.m_IsSkipOpening = true;
                mainGameStage.m_UpdateGoalSequence.Req(new Action(mainGameStage.updateGoalSub_RECREATE));
                return;
            }
            UpdateGoalSubEffectPlus1Original(_thisPtr);
        }
    }
}
