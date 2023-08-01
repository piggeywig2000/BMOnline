using System;
using System.IO;
using System.Linq;
using Flash2;
using Framework;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BMOnline.Mod.Patches
{
    internal class TestPatch
    {
        class LoadingSpinnerAnimation : MonoBehaviour
        {
            public LoadingSpinnerAnimation() : base() { }
            public LoadingSpinnerAnimation(IntPtr handle) : base(handle) { }

            private Image[] images = null;
            private bool isPlaying = false;
            private float timeSincePlay = 0;

            public void Start()
            {
                images = new Image[8];
                for (int i = 0; i < 8; i++)
                {
                    images[i] = transform.GetChild(i).GetComponent<Image>();
                }
            }

            public void Update()
            {
                timeSincePlay += Time.unscaledDeltaTime;

                for (int i = 0; i < 8; i++)
                {
                    Image image = images[i];
                    float animTime = (((timeSincePlay * 6) + (8 - i)) % 8) / 6;
                    float newAlpha = Mathf.Max(1 - animTime, 0);
                    newAlpha *= Mathf.Min(timeSincePlay, 1);
                    if (!isPlaying && newAlpha > image.color.a)
                        newAlpha = 0;
                    if (newAlpha != image.color.a)
                        image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                }
            }

            public void Play()
            {
                isPlaying = true;
                timeSincePlay = 0;
            }

            public void Pause()
            {
                isPlaying = false;
            }
        }

        private static bool hasInit = false;
        private static SelMainMenuSequence sequence = null;
        private static LoadingSpinnerAnimation loadingSpinner = null;

        public static void Update()
        {
            if (!hasInit)
            {
                SceneManager.sceneLoaded = new Action<Scene, LoadSceneMode>((scene, loadSceneMode) =>
                {
                    if (scene.name == "MainMenu")
                    {
                        sequence = UnityEngine.Object.FindObjectOfType<SelMainMenuSequence>();
                    }
                });
                Initialize();
                hasInit = true;
            }

            if (sequence != null && sequence.m_IsCreateWindows)
            {
                AddMainMenuItems();
                sequence = null;
            }
            if (handle != null && Input.GetKey(KeyCode.G))
            {
                GameManager.SetPause(false);
            }
        }

        private static void Initialize()
        {
            try
            {
                IntPtr eGameKindPtr = IL2CPP.GetIl2CppNestedType(IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "Flash2", "MainGameDef"), "eGameKind");
                Il2CppSystem.Type eGameKindType = Il2CppType.TypeFromPointer(eGameKindPtr);
                IntPtr mainGameKindPtr = IL2CPP.GetIl2CppNestedType(IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "Flash2", "SelectorDef"), "MainGameKind");
                Il2CppSystem.Type mainGameKindType = Il2CppType.TypeFromPointer(mainGameKindPtr);

                Il2CppSystem.Reflection.Assembly assembly = Il2CppSystem.AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Assembly-CSharp");
                Il2CppSystem.Type enumRuntimeHelper = assembly.GetType("Framework.EnumRuntimeHelper`1");
                Il2CppSystem.Type erhEgamekind = enumRuntimeHelper.MakeGenericType(new Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[] { eGameKindType }));
                Il2CppSystem.Type erhMaingamekind = enumRuntimeHelper.MakeGenericType(new Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[] { mainGameKindType }));

                Il2CppSystem.Reflection.MethodInfo egkValToNameGetter = erhEgamekind.GetProperty("valueToNameCollection").GetGetMethod();
                Il2CppSystem.Collections.Generic.Dictionary<MainGameDef.eGameKind, string> egkValToName = egkValToNameGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<MainGameDef.eGameKind, string>>();
                egkValToName.Add((MainGameDef.eGameKind)9, "Race");

                Il2CppSystem.Reflection.MethodInfo egkNameToValGetter = erhEgamekind.GetProperty("nameToValueCollection").GetGetMethod();
                Il2CppSystem.Collections.Generic.Dictionary<string, MainGameDef.eGameKind> egkNameToVal = egkNameToValGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<string, MainGameDef.eGameKind>>();
                egkNameToVal.Add("Race", (MainGameDef.eGameKind)9);

                Il2CppSystem.Reflection.MethodInfo mgkValToNameGetter = erhMaingamekind.GetProperty("valueToNameCollection").GetGetMethod();
                Il2CppSystem.Collections.Generic.Dictionary<SelectorDef.MainGameKind, string> mgkValToName = mgkValToNameGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<SelectorDef.MainGameKind, string>>();
                mgkValToName.Add((SelectorDef.MainGameKind)8, "RaceMode");

                Il2CppSystem.Reflection.MethodInfo mgkNameToValGetter = erhMaingamekind.GetProperty("nameToValueCollection").GetGetMethod();
                Il2CppSystem.Collections.Generic.Dictionary<string, SelectorDef.MainGameKind> mgkNameToVal = mgkNameToValGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<string, SelectorDef.MainGameKind>>();
                mgkNameToVal.Add("RaceMode", (SelectorDef.MainGameKind)8);

                PauseDef.s_PauseModeKindCollection.Add((MainGameDef.eGameKind)9, Pause.ModeKind.MainGame_Practice);

                SelPauseWindow.s_MgMenuKindCollection.Add((SelectorDef.MainGameKind)8, SelectorDef.MainMenuKind.MgModeSelect);

                TextData textData = new TextData();
                textData.textDictionary.Add("maingame_onlineracemode", new TextData.Context() { text = "Online Race Mode" });
                textData.textDictionary.Add("tips_main_onlinerace01", new TextData.Context() { text = "Press <sprite name=\"MainGame_QuickRetry\"> in Online Race Mode to retry the stage right away!", isUseTag = true });
                Framework.Text.TextManager.AddData(GameParam.language, textData);

                AssetBundleCache.element_t element = new AssetBundleCache.element_t();
                AssetBundleCache.Instance.m_assetBundleNameToEntityDict.Add("bmonline_assetcache", element);
                AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_tmb_mode_online_race.tga", "bmonline_assetcache");
                element.Load("bmonline_assetcache");

                //Sound
                Sound.Instance.m_cueSheetParamDict.Add((sound_id.cuesheet)101, new Sound.cuesheet_param_t("bmonline_sound", Path.Combine(AssetBundleItems.DllFolder, "bmonline_sound.acb"), Path.Combine(AssetBundleItems.DllFolder, "bmonline_sound.awb")));
                Sound.Instance.m_cueParamDict.Add((sound_id.cue)839, new Sound.cue_param_t((sound_id.cuesheet)101, "w01_jungle"));
                Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)101);

                //Loading spinner
                ClassInjector.RegisterTypeInIl2Cpp<LoadingSpinnerAnimation>();
                GameObject spinnerGo = UnityEngine.Object.Instantiate(AssetBundleItems.LoadingPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").Find("c_system_0").Find("safe_area"));
                loadingSpinner = spinnerGo.transform.GetChild(0).gameObject.AddComponent<LoadingSpinnerAnimation>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed: {e}");
            }
        }

        private static void AddMainMenuItems()
        {
            SelMgModeItemDataListObject dataList = sequence.GetData<SelMgModeItemDataListObject>(SelMainMenuSequence.Data.MgModeSelect);
            if (dataList.m_ItemDataList.Count <= 6)
            {
                SelMgModeItemData itemData = new SelMgModeItemData();
                itemData.transitionMenuKind = SelectorDef.MainMenuKind.HowToPlay;
                itemData.mainGamemode = (SelectorDef.MainGameKind)8;
                itemData.mainGameKind = (MainGameDef.eGameKind)9;
                itemData.textKey = "maingame_onlineracemode";
                itemData.descriptionTextKey = "";
                itemData.isHideText = true;
                itemData.supplementaryTextKey = "";
                itemData.m_ThumbnailSpritePath = new SubAssetSpritePath() { m_Identifier = "ui/t_tmb_mode_online_race.tga:t_tmb_mode_online_race" };
                dataList.m_ItemDataList.Add(itemData);
            }

            SelHowToPlayData howToPlayData = sequence.GetData<SelHowToPlayData>(SelMainMenuSequence.Data.HowToPlay);
            if (howToPlayData.m_PCData.m_MainGameDataArray.Length <= 8)
            {
                SelHowToPlayItemDataListObject[] newHowToPlayArray = new SelHowToPlayItemDataListObject[9];
                Array.Copy(howToPlayData.m_PCData.m_MainGameDataArray, newHowToPlayArray, 8);
                newHowToPlayArray[8] = newHowToPlayArray[4];
                howToPlayData.m_PCData.m_MainGameDataArray = newHowToPlayArray;
                newHowToPlayArray = new SelHowToPlayItemDataListObject[9];
                Array.Copy(howToPlayData.m_ConsoleData.m_MainGameDataArray, newHowToPlayArray, 8);
                newHowToPlayArray[8] = newHowToPlayArray[4];
                howToPlayData.m_ConsoleData.m_MainGameDataArray = newHowToPlayArray;
            }
        }

        private delegate void Test1Delegate(IntPtr _thisPtr);
        private static Test1Delegate Test1Instance;
        private static Test1Delegate Test1Original;

        private delegate IntPtr Test2Delegate(IntPtr _thisPtr, SystemLanguage language, IntPtr key);
        private static Test2Delegate Test2Instance;
        private static Test2Delegate Test2Original;

        public static unsafe void CreateDetour()
        {
            Test1Instance = Test1;
            Test1Original = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(MainGameStage.ReadyGoSequenceNormal).GetMethod(nameof(MainGameStage.ReadyGoSequenceNormal.mdActivatePlayer)))
                .GetValue(null)).MethodPointer, Test1Instance);

            Test2Instance = Test2;
            Test2Original = ClassInjector.Detour.Detour(UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                typeof(Framework.Text.TextManager).GetMethod(nameof(Framework.Text.TextManager.getText)))
                .GetValue(null)).MethodPointer, Test2Instance);
        }

        static Sound.Handle handle = null;
        static void Test1(IntPtr _thisPtr)
        {
            if (MainGame.mainGameStage.gameKind == (MainGameDef.eGameKind)9 && !Input.GetKey(KeyCode.G))
            {
                if (handle == null)
                {
                    GameManager.SetPause(true);
                    GameManager.MainGameBgm.Pause(true);
                    loadingSpinner.Play();
                    handle = new Sound.Handle();
                    handle.Prepare((sound_id.cue)839);
                    handle.Play();
                    handle.volume = 2;
                }
                return;
            }
            if (handle != null)
            {
                GameManager.MainGameBgm.Pause(false);
                loadingSpinner.Pause();
                handle.Stop();
                handle = null;
            }
            Test1Original(_thisPtr);
        }

        static IntPtr Test2(IntPtr _thisPtr, SystemLanguage language, IntPtr key)
        {
            string keyStr = IL2CPP.Il2CppStringToManaged(key);
            if (MainGame.gameKind == (MainGameDef.eGameKind)9 && !string.IsNullOrEmpty(keyStr) && keyStr == "maingame_practice_mode")
            {
                key = IL2CPP.ManagedStringToIl2Cpp("maingame_onlineracemode");
            }
            return Test2Original(_thisPtr, language, key);
        }
    }
}
