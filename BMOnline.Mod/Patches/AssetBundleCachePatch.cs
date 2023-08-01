using System;
using System.IO;
using UnhollowerBaseLib.Runtime;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using Framework;

namespace BMOnline.Mod.Patches
{
    internal static class AssetBundleCachePatch
    {
        private delegate IntPtr GetStreamingAssetFullpathDelegate(IntPtr fileName, bool isUrl);
        private static GetStreamingAssetFullpathDelegate GetStreamingAssetFullpathInstance;
        private static GetStreamingAssetFullpathDelegate GetStreamingAssetFullpathOriginal;

        public static unsafe void CreateDetour()
        {
            GetStreamingAssetFullpathInstance = GetStreamingAssetFullpath;
            GetStreamingAssetFullpathOriginal = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(AssetBundleCache).GetMethod(nameof(AssetBundleCache._get_streaming_asset_fullpath)))
                .GetValue(null)).MethodPointer, GetStreamingAssetFullpathInstance);

        }

        static IntPtr GetStreamingAssetFullpath(IntPtr fileName, bool isUrl)
        {
            if (IL2CPP.Il2CppStringToManaged(fileName) == "bmonline_assetcache")
            {
                return IL2CPP.ManagedStringToIl2Cpp(Path.Combine(AssetBundleItems.DllFolder, "bmonline_assetcache"));
            }
            return GetStreamingAssetFullpathOriginal(fileName, isUrl);
        }
    }
}
