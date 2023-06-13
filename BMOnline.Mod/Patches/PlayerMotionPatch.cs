using System;
using Flash2;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;

namespace BMOnline.Mod.Patches
{
    internal static class PlayerMotionPatch
    {
        public static bool PreventSetState { get; set; } = false;

        private delegate void SetStateDelegate(IntPtr _thisPtr, PlayerMotion.State state);
        private static SetStateDelegate SetStateInstance;
        private static SetStateDelegate SetStateOriginal;

        public static unsafe void CreateDetour()
        {
            SetStateInstance = SetState;
            SetStateOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(PlayerMotion).GetMethod(nameof(PlayerMotion.SetState)))
                .GetValue(null)).MethodPointer, SetStateInstance);

        }

        static void SetState(IntPtr _thisPtr, PlayerMotion.State state)
        {
            PlayerMotion _this = new PlayerMotion(_thisPtr);

            bool canOverrideBlocker = ((int)state & (1 << 20)) > 0; //If special bit is set
            if (!canOverrideBlocker && PreventSetState) { return; }

            state = (PlayerMotion.State)((int)state & ~(1 << 20));

            SetStateOriginal(_thisPtr, state);
        }
    }
}
