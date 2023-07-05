using BMOnline.Client;
using BMOnline.Common.Relay.Requests;
using BMOnline.Common.Relay.Snapshots;
using Flash2;
using UnityEngine;

namespace BMOnline.Mod.Players
{
    internal class LocalPlayer
    {
        private readonly GameState gameState;
        private PlayerInfoRequest lastPlayerInfo;

        private Quaternion lastRotationValue = Quaternion.identity;
        private bool didRotateLastFrame = false;
        private Vector3 lastAngularVelocity = Vector3.zero;

        public LocalPlayer(GameState gameState)
        {
            this.gameState = gameState;
            lastPlayerInfo = new PlayerInfoRequest();
        }

        public void SendPlayerInfo(OnlineState state)
        {
            PlayerInfoRequest playerInfo;
            if (!gameState.IsInGame)
            {
                playerInfo = new PlayerInfoRequest("", byte.MaxValue, byte.MaxValue, ushort.MaxValue, lastPlayerInfo.Character, lastPlayerInfo.SkinIndex, lastPlayerInfo.CustomisationsNum, lastPlayerInfo.CustomisationsChara);
            }
            else
            {
                Player player = gameState.MainGameStage.GetPlayer();
                CharaCustomize.PartsSet partsSet = CharaCustomizeManager.PartsSet.Get(player.m_CharaSelectDatum);
                byte[] customisationsNum = new byte[10];
                byte[] customisationsChara = new byte[10];
                for (int i = 0; i < 10; i++)
                {
                    CharaCustomize.PartsKey partsKey = partsSet.Get((CharaCustomize.eAssignPos)i);
                    customisationsNum[i] = (byte)(partsKey == null ? byte.MaxValue : partsKey.m_Number);
                    customisationsChara[i] = (byte)(partsKey == null ? Chara.eKind.Invalid : partsKey.m_CharaKind);
                }
                playerInfo = new PlayerInfoRequest("",
                    (byte)gameState.MainGameStage.gameKind,
                    (byte)(gameState.MainGameStage.gameKind == MainGameDef.eGameKind.Practice ? MgCourseDataManager.currentPracticeCourse : MgCourseDataManager.currentCourse),
                    (ushort)gameState.MainGameStage.stageIndex,
                    (byte)player.charaKind, (byte)player.charaSkinIndex, customisationsNum, customisationsChara);
            }
            lastPlayerInfo = playerInfo;
            state.GetPlayerInfoType().SendData(playerInfo);
        }

        public void SendStagePosition(OnlineState state)
        {
            Player player = gameState.MainGameStage.GetPlayer();
            StagePositionSnapshot snapshot = new StagePositionSnapshot(
                (player.transform.localPosition.x, player.transform.localPosition.y, player.transform.localPosition.z),
                (lastAngularVelocity.x, lastAngularVelocity.y, lastAngularVelocity.z),
                (byte)player.GetMotion().GetState(),
                player.IsOnGround());
            state.GetStagePositionType().SetSnapshotToSend(snapshot, RelaySnapshotBroadcastType.EveryoneOnStage, (ushort)gameState.MainGameStage.stageIndex);
        }

        public void FixedUpdate()
        {
            //Calculate angular velocity
            if (gameState.IsInGame && gameState.MainGameStage.GetPlayer() != null)
            {
                Player player = gameState.MainGameStage.GetPlayer();
                if (didRotateLastFrame)
                {
                    Quaternion delta = player.transform.rotation * Quaternion.Inverse(lastRotationValue);
                    delta.ToAngleAxis(out float angle, out Vector3 axis);
                    lastAngularVelocity = (axis * angle) / (Time.fixedDeltaTime);
                }
                lastRotationValue = player.transform.rotation;
                didRotateLastFrame = true;
            }
            else
            {
                didRotateLastFrame = false;
            }
        }
    }
}
