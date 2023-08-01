using System;
using System.Linq;
using Flash2;
using UnityEngine.SceneManagement;
using UnityEngine;
using Framework;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using System.IO;
using BMOnline.Mod.Patches;

namespace BMOnline.Mod
{
    internal class RaceManager
    {
        private readonly LoadingSpinner loadingSpinner;
        private SelMainMenuSequence sequence = null;
        private Sound.Handle handle = null;

        public void ChangeLoading(bool value)
        {
            if (value == (handle != null))
                return;
            GameManager.SetPause(value);
            GameManager.MainGameBgm.Pause(value);
            loadingSpinner.SetPlaying(value);
            if (value)
            {
                handle = new Sound.Handle();
                handle.Prepare((sound_id.cue)839);
                handle.Play();
                handle.volume = 2;
            }
            else if (!value)
            {
                handle.Stop();
                handle = null;
            }
        }

        public RaceManager(LoadingSpinner loadingSpinner)
        {
            this.loadingSpinner = loadingSpinner;
            MainGameStagePatch.RaceManager = this;
            MainGamePatch.RaceStageId = 2201;

            SceneManager.sceneLoaded = new Action<Scene, LoadSceneMode>((scene, loadSceneMode) =>
            {
                if (scene.name == "MainMenu")
                {
                    sequence = UnityEngine.Object.FindObjectOfType<SelMainMenuSequence>();
                }
            });

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
            textData.textDictionary.Add("maingame_howtoplay_onlinerace", new TextData.Context() { text = "Race against online players to reach the goal first. Everyone starts at the same time and the first to reach the goal wins. Press <sprite name=\"MainGame_QuickRetry\"> to retry the stage right away.", isUseTag = true });
            textData.textDictionary.Add("tips_main_onlinerace01", new TextData.Context() { text = "Press <sprite name=\"MainGame_QuickRetry\"> in Online Race Mode to retry the stage right away!", isUseTag = true });
            Framework.Text.TextManager.AddData(GameParam.language, textData);

            AssetBundleCache.element_t element = new AssetBundleCache.element_t();
            AssetBundleCache.Instance.m_assetBundleNameToEntityDict.Add("bmonline_assetcache", element);
            AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_tmb_mode_online_race.tga", "bmonline_assetcache");
            AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_sousa_main_online_race.tga", "bmonline_assetcache");
            element.Load("bmonline_assetcache");

            //Sound
            Sound.Instance.m_cueSheetParamDict.Add((sound_id.cuesheet)101, new Sound.cuesheet_param_t("bmonline_sound", Path.Combine(AssetBundleItems.DllFolder, "bmonline_sound.acb"), Path.Combine(AssetBundleItems.DllFolder, "bmonline_sound.awb")));
            Sound.Instance.m_cueParamDict.Add((sound_id.cue)839, new Sound.cue_param_t((sound_id.cuesheet)101, "mkwii_wifi"));
            Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)101);
        }

        private void AddMainMenuItems()
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
                SelHTPMainGame pageDataListObject = newHowToPlayArray[8].m_PageDataListObject.Cast<SelHTPMainGame>();
                Il2CppSystem.Collections.Generic.List<SelHowToPlayPageData> ruleExplanationlist = new Il2CppSystem.Collections.Generic.List<SelHowToPlayPageData>();
                ruleExplanationlist.Add(new SelHowToPlayPageData()
                {
                    m_TextReference = new Framework.Text.TextReference() { m_Key = "maingame_howtoplay_onlinerace" },
                    m_Sprite = "ui/t_sousa_main_online_race.tga"
                });
                pageDataListObject.m_Collection.Add((MainGameDef.eGameKind)9, new SelHTPMainGame.ModePageData()
                {
                    m_RuleExplanationList = new SelHowToPlayPageDataListObject.PageDataList() { list = ruleExplanationlist }
                });
                howToPlayData.m_PCData.m_MainGameDataArray = newHowToPlayArray;

                newHowToPlayArray = new SelHowToPlayItemDataListObject[9];
                Array.Copy(howToPlayData.m_ConsoleData.m_MainGameDataArray, newHowToPlayArray, 8);
                newHowToPlayArray[8] = newHowToPlayArray[4];
                howToPlayData.m_ConsoleData.m_MainGameDataArray = newHowToPlayArray;
            }
        }

        public void Update()
        {
            if (sequence != null && sequence.m_IsCreateWindows)
            {
                AddMainMenuItems();
                sequence = null;
            }
            if (handle != null && Input.GetKey(KeyCode.G))
            {
                GameManager.SetPause(false);
            }

            if (MainGame.mainGameStage != null && Input.GetKeyDown(KeyCode.N))
            {
                MainGame.mainGameStage.m_UpdateGoalSequence.Req(null);
                MainGame.mainGameStage.m_ReadyGoSequenceNormal?.m_Sequence?.Req(null);
                ChangeLoading(false);
                MainGamePatch.RaceStageId = 2005;
                AppScene.ChangeReq(AppScene.eID.MainGame, AppScene.FadeOperation.Animator);
            }
        }
    }
}
