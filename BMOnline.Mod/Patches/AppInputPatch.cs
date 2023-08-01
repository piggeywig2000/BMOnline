using System;
using Flash2;
using UnhollowerBaseLib.Runtime;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace BMOnline.Mod.Patches
{
    internal static class AppInputPatch
    {
        public static bool PreventKeyboardUpdate { get; set; } = false;

        private delegate void KeyboardParamUpdateDelegate(IntPtr _thisPtr, IntPtr isAnyKey, IntPtr isAnyKeyDown, IntPtr isAnyKeyUp);
        private static KeyboardParamUpdateDelegate KeyboardParamUpdateInstance;
        private static KeyboardParamUpdateDelegate KeyboardParamUpdateOriginal;

        public static unsafe void CreateDetour()
        {
            KeyboardParamUpdateInstance = KeyboardParamUpdate;
            KeyboardParamUpdateOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(AppInput).GetMethod(nameof(AppInput.keyboard_param_update)))
                .GetValue(null)).MethodPointer, KeyboardParamUpdateInstance);
        }

        static void KeyboardParamUpdate(IntPtr _thisPtr, IntPtr isAnyKey, IntPtr isAnyKeyDown, IntPtr isAnyKeyUp)
        {
            if (!PreventKeyboardUpdate)
                KeyboardParamUpdateOriginal(_thisPtr, isAnyKey, isAnyKeyDown, isAnyKeyUp);
        }
    }
}
