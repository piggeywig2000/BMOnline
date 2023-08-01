using System;
using Flash2;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;

namespace BMOnline.Mod.Patches
{
    internal static class MainGamePatch
    {
        public static int RaceStageId { private get; set; } = 0;

        private delegate IntPtr StartDelegate(IntPtr _thisPtr);
        private static StartDelegate StartInstance;
        private static StartDelegate StartOriginal;

        private delegate void MdLoadResidentWaitDelegate(IntPtr _thisPtr);
        private static MdLoadResidentWaitDelegate MdLoadResidentWaitInstance;
        private static MdLoadResidentWaitDelegate MdLoadResidentWaitOriginal;

        public static unsafe void CreateDetour()
        {
            StartInstance = Start;
            StartOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGame).GetMethod(nameof(MainGame.Start)))
                .GetValue(null)).MethodPointer, StartInstance);

            MdLoadResidentWaitInstance = md_load_resident_wait;
            MdLoadResidentWaitOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGame).GetMethod(nameof(MainGame.md_load_resident_wait)))
                .GetValue(null)).MethodPointer, MdLoadResidentWaitInstance);
        }

        static IntPtr Start(IntPtr _thisPtr)
        {
            if (GameParam.selectorParam.selectedGameKind == (MainGameDef.eGameKind)9)
            {
                GameParam.selectorParam.selectedCourse = MainGameDef.eCourse.Invalid;
                GameParam.selectorParam.selectedStageIndex = RaceStageId;
                MgCourseDataManager.SetCurrentCourse(MainGameDef.eCourse.Invalid, 0);
            }
            return StartOriginal(_thisPtr);
        }

        static void md_load_resident_wait(IntPtr _thisPtr)
        {
            if (Pause.Exists && Pause.Instance.currentData?.tips?.m_MgTipsArray != null && Pause.Instance.currentData.tips.m_MgTipsArray.Length <= 8)
            {
                SelTipsObject.TipArray[] newTips = new SelTipsObject.TipArray[10];
                Array.Copy(Pause.Instance.currentData.tips.m_MgTipsArray, newTips, 8);
                newTips[9] = new SelTipsObject.TipArray() { m_TipsArray = new SelTipsElement[1] { new SelTipsElement() { m_Text = new Framework.Text.TextReference() { m_Key = "tips_main_onlinerace01" } } } };
                Pause.Instance.currentData.tips.m_MgTipsArray = new Il2CppReferenceArray<SelTipsObject.TipArray>(newTips);
            }
            if (Pause.Exists && Pause.Instance.currentData?.m_ItemDataList != null && MainGame.gameKind == (MainGameDef.eGameKind)9 && Pause.Instance.currentData.m_ItemDataList.Length >= 8)
            {
                Pause.Instance.currentData.m_ItemDataList.RemoveAt(2);
            }
            MdLoadResidentWaitOriginal(_thisPtr);
        }
    }
}
