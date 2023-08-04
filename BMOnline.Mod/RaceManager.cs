using System;
using System.IO;
using System.Linq;
using BMOnline.Client;
using BMOnline.Common.Relay.Snapshots;
using BMOnline.Common.Gamemodes;
using BMOnline.Common.Messaging;
using BMOnline.Mod.Patches;
using Flash2;
using Framework;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine.SceneManagement;

namespace BMOnline.Mod
{
    internal class RaceManager
    {
        private readonly LoadingSpinner loadingSpinner;
        private readonly GameState gameState;
        private readonly OnlineClient client;

        private SelMainMenuSequence sequence = null;
        private Sound.Handle handle = null;
        private RaceStateSnapshot lastState;
        private RaceStateUpdateMessage lastUpdate;
        private ushort currentStage;

        public RaceManager(LoadingSpinner loadingSpinner, GameState gameState, OnlineClient client)
        {
            this.loadingSpinner = loadingSpinner;
            this.gameState = gameState;
            this.client = client;
            MainGameStagePatch.RaceManager = this;

            Reset();

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
            egkValToName.Add((MainGameDef.eGameKind)OnlineGamemode.RaceMode, "OnlineRace");
            egkValToName.Add((MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode, "OnlineTimeAttack");

            Il2CppSystem.Reflection.MethodInfo egkNameToValGetter = erhEgamekind.GetProperty("nameToValueCollection").GetGetMethod();
            Il2CppSystem.Collections.Generic.Dictionary<string, MainGameDef.eGameKind> egkNameToVal = egkNameToValGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<string, MainGameDef.eGameKind>>();
            egkNameToVal.Add("OnlineRace", (MainGameDef.eGameKind)OnlineGamemode.RaceMode);
            egkNameToVal.Add("OnlineTimeAttack", (MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode);

            Il2CppSystem.Reflection.MethodInfo mgkValToNameGetter = erhMaingamekind.GetProperty("valueToNameCollection").GetGetMethod();
            Il2CppSystem.Collections.Generic.Dictionary<SelectorDef.MainGameKind, string> mgkValToName = mgkValToNameGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<SelectorDef.MainGameKind, string>>();
            mgkValToName.Add((SelectorDef.MainGameKind)8, "OnlineRaceMode");
            mgkValToName.Add((SelectorDef.MainGameKind)9, "OnlineTimeAttackMode");

            Il2CppSystem.Reflection.MethodInfo mgkNameToValGetter = erhMaingamekind.GetProperty("nameToValueCollection").GetGetMethod();
            Il2CppSystem.Collections.Generic.Dictionary<string, SelectorDef.MainGameKind> mgkNameToVal = mgkNameToValGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<string, SelectorDef.MainGameKind>>();
            mgkNameToVal.Add("OnlineRaceMode", (SelectorDef.MainGameKind)8);
            mgkNameToVal.Add("OnlineTimeAttackMode", (SelectorDef.MainGameKind)9);

            PauseDef.s_PauseModeKindCollection.Add((MainGameDef.eGameKind)OnlineGamemode.RaceMode, Pause.ModeKind.MainGame_Practice);
            PauseDef.s_PauseModeKindCollection.Add((MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode, Pause.ModeKind.MainGame_Practice);

            SelPauseWindow.s_MgMenuKindCollection.Add((SelectorDef.MainGameKind)8, SelectorDef.MainMenuKind.MgModeSelect);

            TextData textData = new TextData();
            textData.textDictionary.Add("maingame_onlineracemode", new TextData.Context() { text = "Online Race Mode" });
            textData.textDictionary.Add("maingame_onlinetimeattackmode", new TextData.Context() { text = "Online Time Attack Mode" });
            textData.textDictionary.Add("maingame_howtoplay_onlinerace", new TextData.Context() { text = "Race against online players to reach the goal first. Everyone starts at the same time and the first to reach the goal wins. Press <sprite name=\"MainGame_QuickRetry\"> to retry the stage right away.", isUseTag = true });
            textData.textDictionary.Add("maingame_howtoplay_onlinetimeattack", new TextData.Context() { text = "Race against online players to reach the goal in the shortest time. The player with the best time when the time limit is reached wins. Press <sprite name=\"MainGame_QuickRetry\"> to retry the stage right away.", isUseTag = true });
            textData.textDictionary.Add("tips_main_onlinerace01", new TextData.Context() { text = "Press <sprite name=\"MainGame_QuickRetry\"> in Online Race Mode to retry the stage right away!", isUseTag = true });
            textData.textDictionary.Add("tips_main_onlinetimeattack01", new TextData.Context() { text = "Press <sprite name=\"MainGame_QuickRetry\"> in Online Time Attack Mode to retry the stage right away!", isUseTag = true });
            Framework.Text.TextManager.AddData(GameParam.language, textData);

            AssetBundleCache.element_t element = new AssetBundleCache.element_t();
            AssetBundleCache.Instance.m_assetBundleNameToEntityDict.Add("bmonline_assetcache", element);
            AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_tmb_mode_online_race.tga", "bmonline_assetcache");
            AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_tmb_mode_online_timeattack.tga", "bmonline_assetcache");
            AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_sousa_main_online_race.tga", "bmonline_assetcache");
            AssetBundleCache.Instance.m_pathToAssetBundleNameDict.Add("ui/t_sousa_main_online_timeattack.tga", "bmonline_assetcache");
            element.Load("bmonline_assetcache");

            //Sound
            Sound.Instance.m_cueSheetParamDict.Add((sound_id.cuesheet)101, new Sound.cuesheet_param_t("bmonline_sound", Path.Combine(AssetBundleItems.DllFolder, "bmonline_sound.acb"), Path.Combine(AssetBundleItems.DllFolder, "bmonline_sound.awb")));
            Sound.Instance.m_cueParamDict.Add((sound_id.cue)839, new Sound.cue_param_t((sound_id.cuesheet)101, "mkwii_wifi"));
            Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)101);
        }

        public float TimeRemaining { get; private set; }

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

        private void AddMainMenuItems()
        {
            SelMgModeItemDataListObject dataList = sequence.GetData<SelMgModeItemDataListObject>(SelMainMenuSequence.Data.MgModeSelect);
            if (dataList.m_ItemDataList.Count <= 6)
            {
                SelMgModeItemData itemData = new SelMgModeItemData();
                itemData.transitionMenuKind = SelectorDef.MainMenuKind.HowToPlay;
                itemData.mainGamemode = (SelectorDef.MainGameKind)8;
                itemData.mainGameKind = (MainGameDef.eGameKind)OnlineGamemode.RaceMode;
                itemData.textKey = "maingame_onlineracemode";
                itemData.descriptionTextKey = "";
                itemData.isHideText = true;
                itemData.supplementaryTextKey = "";
                itemData.m_ThumbnailSpritePath = new SubAssetSpritePath() { m_Identifier = "ui/t_tmb_mode_online_race.tga:t_tmb_mode_online_race" };
                dataList.m_ItemDataList.Add(itemData);

                itemData = new SelMgModeItemData();
                itemData.transitionMenuKind = SelectorDef.MainMenuKind.HowToPlay;
                itemData.mainGamemode = (SelectorDef.MainGameKind)8;
                itemData.mainGameKind = (MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode;
                itemData.textKey = "maingame_onlinetimeattackmode";
                itemData.descriptionTextKey = "";
                itemData.isHideText = true;
                itemData.supplementaryTextKey = "";
                itemData.m_ThumbnailSpritePath = new SubAssetSpritePath() { m_Identifier = "ui/t_tmb_mode_online_timeattack.tga:t_tmb_mode_online_timeattack" };
                dataList.m_ItemDataList.Add(itemData);
            }

            SelHowToPlayData howToPlayData = sequence.GetData<SelHowToPlayData>(SelMainMenuSequence.Data.HowToPlay);
            if (howToPlayData.m_PCData.m_MainGameDataArray.Length <= 8)
            {
                SelHowToPlayItemDataListObject[] newHowToPlayArray = new SelHowToPlayItemDataListObject[10];
                Array.Copy(howToPlayData.m_PCData.m_MainGameDataArray, newHowToPlayArray, 8);

                newHowToPlayArray[8] = newHowToPlayArray[4];
                SelHTPMainGame pageDataListObject = newHowToPlayArray[8].m_PageDataListObject.Cast<SelHTPMainGame>();
                Il2CppSystem.Collections.Generic.List<SelHowToPlayPageData> ruleExplanationlist = new Il2CppSystem.Collections.Generic.List<SelHowToPlayPageData>();
                ruleExplanationlist.Add(new SelHowToPlayPageData()
                {
                    m_TextReference = new Framework.Text.TextReference() { m_Key = "maingame_howtoplay_onlinerace" },
                    m_Sprite = "ui/t_sousa_main_online_race.tga"
                });
                pageDataListObject.m_Collection.Add((MainGameDef.eGameKind)OnlineGamemode.RaceMode, new SelHTPMainGame.ModePageData()
                {
                    m_RuleExplanationList = new SelHowToPlayPageDataListObject.PageDataList() { list = ruleExplanationlist }
                });

                newHowToPlayArray[9] = newHowToPlayArray[4];
                pageDataListObject = newHowToPlayArray[9].m_PageDataListObject.Cast<SelHTPMainGame>();
                ruleExplanationlist = new Il2CppSystem.Collections.Generic.List<SelHowToPlayPageData>();
                ruleExplanationlist.Add(new SelHowToPlayPageData()
                {
                    m_TextReference = new Framework.Text.TextReference() { m_Key = "maingame_howtoplay_onlinetimeattack" },
                    m_Sprite = "ui/t_sousa_main_online_timeattack.tga"
                });
                pageDataListObject.m_Collection.Add((MainGameDef.eGameKind)OnlineGamemode.TimeAttackMode, new SelHTPMainGame.ModePageData()
                {
                    m_RuleExplanationList = new SelHowToPlayPageDataListObject.PageDataList() { list = ruleExplanationlist }
                });

                howToPlayData.m_PCData.m_MainGameDataArray = newHowToPlayArray;

                newHowToPlayArray = new SelHowToPlayItemDataListObject[10];
                Array.Copy(howToPlayData.m_ConsoleData.m_MainGameDataArray, newHowToPlayArray, 8);
                newHowToPlayArray[8] = newHowToPlayArray[4];
                newHowToPlayArray[9] = newHowToPlayArray[4];
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
        }

        public float GetFinishTime()
        {
            if (gameState.MainGameStage == null)
                return 0f;
            if (gameState.MainGameStage.m_GoalTime == 0)
                return 0f;
            return Flash2.Util.FrameToSec(gameState.MainGameStage.m_MaxGameTime - gameState.MainGameStage.m_GoalTime);
        }

        public RaceStateUpdateMessage GetUpdateToSend() => new RaceStateUpdateMessage()
        {
            Stage = currentStage,
            IsLoaded = MainGameStagePatch.DidBlockLoadThisFrame ||
                (gameState.MainGameStage != null && gameState.MainGameStage.state == MainGameStage.State.IDLE && gameState.MainGameStage.state != MainGameStage.State.PREPARE),
            FinishTime = GetFinishTime()
        };

        private void Reset()
        {
            lastState = null;
            lastUpdate = null;
            currentStage = 0;
            TimeRemaining = 0;
        }

        public void LateUpdateFromState()
        {
            if ((byte)MainGame.gameKind != (byte)OnlineGamemode.RaceMode && (byte)MainGame.gameKind != (byte)OnlineGamemode.TimeAttackMode)
            {
                if (client.State.GetRaceStateType().GetLatestSnapshot(0) != null)
                {
                    Reset();
                    client.State.GetRaceStateType().ClearPlayerSnapshots(0);
                    client.State.SetRaceState(null);
                }
                return;
            }

            RaceStateSnapshot raceStateSnapshot = (RaceStateSnapshot)client.State.GetRaceStateType().GetCurrentSnapshot(0, client.Time);
            if (raceStateSnapshot == null)
                return;

            TimeRemaining = raceStateSnapshot.TimeRemaining;

            if ((lastState == null && raceStateSnapshot.Stage != 0) || raceStateSnapshot.Stage != lastState.Stage)
            {
                MainGamePatch.RaceStageId = raceStateSnapshot.Stage;
                currentStage = raceStateSnapshot.Stage;
                if (lastState != null)
                {
                    ChangeLoading(false);
                    MainGame.mainGameStage.m_SelectedResultButton = MgResultMenu.eTextKind.Next;
                    MainGame.mainGameStage.m_State = MainGameStage.State.GOAL;
                    MainGame.mainGameStage.m_SubState = 4;
                    MainGame.mainGameStage.m_SubStateTimer = 0;
                    MainGame.mainGameStage.m_UpdateGoalSequence.Req(new Action(MainGame.mainGameStage.updateGoalSub_RECREATE));
                }
            }

            if (lastState == null || raceStateSnapshot.State != lastState.State)
            {
                MainGameStagePatch.ShouldPreventLoad = raceStateSnapshot.State == RaceState.WaitingForLoad;

                if (raceStateSnapshot.State != RaceState.WaitingForLoad && MainGameStagePatch.DidBlockLoadThisFrame)
                {
                    GameManager.SetPause(false);
                }
            }

            if (raceStateSnapshot.State == RaceState.Finished && gameState.MainGameStage != null && gameState.MainGameStage.state == MainGameStage.State.GAME)
            {
                gameState.MainGameStage.m_State = MainGameStage.State.TIMEUP;
                gameState.MainGameStage.m_SubState = 0;
                gameState.MainGameStage.m_StateFrame = 0;
                gameState.MainGameStage.m_StateTimer = 0;
                gameState.MainGameStage.m_SubStateTimer = 0;
            }
            MainGameStagePatch.ShouldPreventFinish = raceStateSnapshot.State == RaceState.Finished;

            if (MainGameStagePatch.DidBlockLoadThisFrame && (lastUpdate == null || lastUpdate.Stage != currentStage || !lastUpdate.IsLoaded))
            {
                RaceStateUpdateMessage updateToSend = GetUpdateToSend();
                client.State.SetRaceState(updateToSend);
                lastUpdate = updateToSend;
            }

            if (gameState.MainGameStage != null && gameState.MainGameStage.m_GoalTime > 0 && (lastUpdate == null || lastUpdate.FinishTime != GetFinishTime()))
            {
                RaceStateUpdateMessage updateToSend = GetUpdateToSend();
                client.State.SetRaceState(updateToSend);
                lastUpdate = updateToSend;
            }

            if (SaveData.userParam.optionParam.IsEnableJump())
                SaveData.userParam.optionParam.SetEnableJump(false);

            if (raceStateSnapshot.State != RaceState.Playing)
            {
                if (Pause.isEnable)
                {
                    Pause.Disable(false);
                }

                if (MainGame.isViewStage)
                {
                    MainGame.Instance.m_viewStageCamera.m_updateMode.ReqExec(new Action(MainGame.Instance.m_viewStageCamera.mdEnd));
                }

                if (MainGame.isViewPlayer)
                {
                    MainGame.Instance.m_photoModeCamera.m_updateMode.ReqExec(new Action(MainGame.Instance.m_photoModeCamera.mdEnd));
                }
            }

            lastState = raceStateSnapshot;
        }
    }
}
