using System;
using BMOnline.Common.Relay.Requests;
using BMOnline.Common.Relay.Snapshots;
using BMOnline.Mod.Settings;
using Flash2;
using Framework;
using UnityEngine;

namespace BMOnline.Mod.Players
{
    internal class NetPlayer : IOnlinePlayer
    {
        private readonly IBmoSettings settings;
        private bool isLoading = false;

        public event EventHandler OnPlayerLoaded;
        public event EventHandler OnPlayerUnloaded;
        public event EventHandler OnPlayerUpdated;

        public NetPlayer(IBmoSettings settings, ushort id, string name)
        {
            this.settings = settings;
            Id = id;
            Name = name;
            Mode = MainGameDef.eGameKind.Invalid;
            Course = MainGameDef.eCourse.Invalid;
            Stage = ushort.MaxValue;
            SelectedCharacter = new Chara.SelectDatum() { m_CharaKind = Chara.eKind.Aiai, m_SkinIndex = 0 };
        }

        public ushort Id { get; }
        public string Name { get; }
        public MainGameDef.eGameKind Mode { get; private set; }
        public MainGameDef.eCourse Course { get; private set; }
        public ushort? Stage { get; private set; }
        public Chara.SelectDatum SelectedCharacter { get; private set; }
        public CharaCustomize.PartsSet Customisations { get; private set; }

        private PlayerGameInfo gameInfo;

        public IPlayerGameInfo GameInfo { get => gameInfo; }

        private CharaCustomize.PartsSet GetPartsSetFromArray(byte[] customisationsNum, byte[] customisationsChara)
        {
            CharaCustomize.PartsSet returnSet = new CharaCustomize.PartsSet();
            for (int i = 0; i < 10; i++)
            {
                if (customisationsNum[i] != byte.MaxValue)
                {
                    returnSet.Set(new CharaCustomize.PartsKey() { m_CharaKind = (Chara.eKind)customisationsChara[i], m_AssignPos = (CharaCustomize.eAssignPos)i, m_Number = customisationsNum[i] });
                }
            }
            return returnSet;
        }

        public void AddNewPlayerInfo(PlayerInfoRequest playerInfo)
        {
            Mode = playerInfo.Mode != byte.MaxValue ? (MainGameDef.eGameKind)playerInfo.Mode : MainGameDef.eGameKind.Invalid;
            Course = playerInfo.Course != byte.MaxValue ? (MainGameDef.eCourse)playerInfo.Course : MainGameDef.eCourse.Invalid;
            Stage = playerInfo.Stage != ushort.MaxValue ? playerInfo.Stage : null;
            SelectedCharacter = new Chara.SelectDatum() { m_CharaKind = playerInfo.Character != byte.MaxValue ? (Chara.eKind)playerInfo.Character : Chara.eKind.Aiai, m_SkinIndex = playerInfo.SkinIndex };

            //If character doesn't exist default to AiAi
            if (string.IsNullOrEmpty(MgCharaManager.Instance.getCharaDatum(SelectedCharacter.mainGameCharaId)?.m_prefabPath) || !AssetBundleCache.AssetExists(MgCharaManager.Instance.getCharaDatum(SelectedCharacter.mainGameCharaId).m_prefabPath))
            {
                SelectedCharacter = new Chara.SelectDatum() { m_CharaKind = Chara.eKind.Aiai, m_SkinIndex = 0 };
            }
            Customisations = GetPartsSetFromArray(playerInfo.CustomisationsNum, playerInfo.CustomisationsChara);

            //If customisation is invalid use default
            Customisations.ForEachPartsKey(new Action<CharaCustomize.PartsKey>((partsKey) =>
            {
                if (CharaCustomizeManager.GetPartsInfo(partsKey) == null)
                {
                    Customisations = CharaCustomizeManager.GetDefaultPartsSet(SelectedCharacter.m_CharaKind);
                }
            }));

            OnPlayerUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Destroy()
        {
            if (gameInfo != null)
            {
                gameInfo.Destroy();
                gameInfo = null;
                OnPlayerUnloaded?.Invoke(this, EventArgs.Empty);
            }
            isLoading = false;
        }

        public void FixedUpdate() => gameInfo?.FixedUpdate();

        public void Update() => gameInfo?.Update();

        public void LateUpdateFromSnapshot(StagePositionSnapshot snapshot) => gameInfo?.LateUpdateFromSnapshot(snapshot);

        public void LateUpdateOutsideState(ushort localPlayerStage, GameState gameState, Transform objRoot)
        {
            //Destroy if we're not in the same stage
            if (Stage != localPlayerStage && (GameInfo != null || isLoading))
                Destroy();

            //Start loading AssetBundles if we haven't yet
            if (gameState.IsInGame && Stage == localPlayerStage && GameInfo == null && !isLoading)
            {
                CharaCustomizeManager.PartsSet.Load(Customisations);
                MgCharaManager.PreloadReq(SelectedCharacter.mainGameCharaId);
                isLoading = true;
            }

            //Instantiate if we're done loading
            if (gameState.IsInGame && Stage == localPlayerStage && isLoading && !CharaCustomizeManager.isBusy && !MgCharaManager.isBusy)
            {
                isLoading = false;
                gameInfo = new PlayerGameInfo(settings, Name, SelectedCharacter, Customisations, objRoot, gameState.MainGameStage);
                OnPlayerLoaded?.Invoke(this, EventArgs.Empty);
            }

            //Update the GameInfo
            gameInfo?.LateUpdate(gameState.MainGameStage);

            if (gameState.IsInGame && gameState.WasMainStageReset)
                gameInfo?.Reset(gameState.MainGameStage);
        }
    }
}
