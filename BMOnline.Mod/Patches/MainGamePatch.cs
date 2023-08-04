using System;
using BMOnline.Common.Gamemodes;
using Flash2;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;

namespace BMOnline.Mod.Patches
{
    internal static class MainGamePatch
    {
        public static int RaceStageId { private get; set; } = 0;

        private delegate bool StartDelegate(IntPtr _thisPtr);
        private static StartDelegate StartInstance;
        private static StartDelegate StartOriginal;

        private delegate void MdMainDelegate(IntPtr _thisPtr);
        private static MdMainDelegate MdMainInstance;
        private static MdMainDelegate MdMainOriginal;

        private delegate void MdLoadResidentWaitDelegate(IntPtr _thisPtr);
        private static MdLoadResidentWaitDelegate MdLoadResidentWaitInstance;
        private static MdLoadResidentWaitDelegate MdLoadResidentWaitOriginal;

        public static unsafe void CreateDetour()
        {
            StartInstance = Start;
            StartOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGame._Start_d__45).GetMethod(nameof(MainGame._Start_d__45.MoveNext)))
                .GetValue(null)).MethodPointer, StartInstance);

            MdMainInstance = md_main;
            MdMainOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGame).GetMethod(nameof(MainGame.md_main)))
                .GetValue(null)).MethodPointer, MdMainInstance);

            MdLoadResidentWaitInstance = md_load_resident_wait;
            MdLoadResidentWaitOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGame).GetMethod(nameof(MainGame.md_load_resident_wait)))
                .GetValue(null)).MethodPointer, MdLoadResidentWaitInstance);
        }

        static bool Start(IntPtr _thisPtr)
        {
            if (GameParam.selectorParam.selectedGameKind == (MainGameDef.eGameKind)OnlineGamemode.RaceMode || GameParam.selectorParam.selectedGameKind == (MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode)
            {
                if (RaceStageId == 0)
                {
                    new MainGame._Start_d__45(_thisPtr).__4__this.m_GameKind = GameParam.selectorParam.selectedGameKind;
                    return true;
                }
                GameParam.selectorParam.selectedCourse = MainGameDef.eCourse.Invalid;
                GameParam.selectorParam.selectedStageIndex = RaceStageId;
                MgCourseDataManager.SetCurrentCourse(MainGameDef.eCourse.Invalid, 0);
            }
            return StartOriginal(_thisPtr);
        }

        static void md_main(IntPtr _thisPtr)
        {
            if (GameParam.selectorParam.selectedGameKind == (MainGameDef.eGameKind)OnlineGamemode.RaceMode || GameParam.selectorParam.selectedGameKind == (MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode)
            {
                MainGame mainGame = new MainGame(_thisPtr);
                if ((mainGame.m_GameInfo == null || !mainGame.m_GameInfo.m_isLoading) && mainGame.m_isRequestRecreateStage)
                {
                    if (RaceStageId == 0)
                    {
                        mainGame.m_isRequestRecreateStage = false;
                        MdMainOriginal(_thisPtr);
                        mainGame.m_isRequestRecreateStage = true;
                        return;
                    }
                    GameParam.selectorParam.selectedStageIndex = RaceStageId;
                }
            }
            MdMainOriginal(_thisPtr);
        }

        static void md_load_resident_wait(IntPtr _thisPtr)
        {
            if (Pause.Exists && Pause.Instance.currentData?.tips?.m_MgTipsArray != null && Pause.Instance.currentData.tips.m_MgTipsArray.Length <= 8)
            {
                SelTipsObject.TipArray[] newTips = new SelTipsObject.TipArray[11];
                Array.Copy(Pause.Instance.currentData.tips.m_MgTipsArray, newTips, 8);
                newTips[9] = new SelTipsObject.TipArray() { m_TipsArray = new SelTipsElement[1] { new SelTipsElement() { m_Text = new Framework.Text.TextReference() { m_Key = "tips_main_onlinerace01" } } } };
                newTips[10] = new SelTipsObject.TipArray() { m_TipsArray = new SelTipsElement[1] { new SelTipsElement() { m_Text = new Framework.Text.TextReference() { m_Key = "tips_main_onlinetimeattack01" } } } };
                Pause.Instance.currentData.tips.m_MgTipsArray = new Il2CppReferenceArray<SelTipsObject.TipArray>(newTips);
            }
            MainGame mainGame = new MainGame(_thisPtr);
            if (Pause.Exists && Pause.Instance.currentData?.m_ItemDataList != null && (mainGame.m_GameKind == (MainGameDef.eGameKind)OnlineGamemode.RaceMode || mainGame.m_GameKind == (MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode) && Pause.Instance.currentData.m_ItemDataList.Length >= 8)
            {
                Pause.Instance.currentData.m_ItemDataList.RemoveAt(5);
                Pause.Instance.currentData.m_ItemDataList.RemoveAt(3);
                Pause.Instance.currentData.m_ItemDataList.RemoveAt(2);
            }
            MdLoadResidentWaitOriginal(_thisPtr);
        }
    }
}
