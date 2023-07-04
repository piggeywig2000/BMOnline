using System;
using System.Collections.Generic;
using System.Linq;
using BMOnline.Client;
using BMOnline.Common.Relay.Requests;
using BMOnline.Common.Relay.Snapshots;
using BMOnline.Mod.Settings;
using UnityEngine;

namespace BMOnline.Mod.Players
{
    internal class NetPlayerManager : IOnlinePlayerManager
    {
        private readonly IBmoSettings settings;
        private readonly GameState gameState;
        private readonly OnlineClient client;
        private readonly LocalPlayer localPlayer;
        private readonly Dictionary<ushort, NetPlayer> idToPlayer = new Dictionary<ushort, NetPlayer>();

        private ushort currentStageId = ushort.MaxValue;
        private Transform objRoot = null;

        public event EventHandler<OnlinePlayerEventArgs> OnPlayerConnected;
        public event EventHandler<OnlinePlayerEventArgs> OnPlayerDisconnected;

        public NetPlayerManager(IBmoSettings settings, GameState gameState, OnlineClient client)
        {
            this.settings = settings;
            this.gameState = gameState;
            this.client = client;
            localPlayer = new LocalPlayer(gameState);

            client.StateSemaphore.Wait();
            client.State.GetPlayerInfoType().OnPlayerUpdated += (s, e) =>
            {
                if (idToPlayer.TryGetValue(e.PlayerId, out NetPlayer netPlayer))
                {
                    PlayerInfoRequest data = (PlayerInfoRequest)e.Data;
                    //If the stage changed, clear the player's stage position snapshots
                    if (data.Stage != netPlayer.Stage)
                        client.State.GetStagePositionType().ClearPlayerSnapshots(netPlayer.Id);
                    netPlayer.AddNewPlayerInfo(data);
                }
            };
            client.StateSemaphore.Release();
        }

        public IReadOnlyCollection<IOnlinePlayer> GetAllPlayers() => idToPlayer.Values;

        public IReadOnlyCollection<IOnlinePlayer> GetLoadedPlayers() => idToPlayer.Values.Where(p => p.GameInfo != null).ToList();

        public IOnlinePlayer GetPlayer(ushort id) => idToPlayer.TryGetValue(id, out NetPlayer netPlayer) ? netPlayer : null;

        public void FixedUpdate()
        {
            localPlayer.FixedUpdate();
            foreach (NetPlayer player in GetLoadedPlayers())
            {
                player.FixedUpdate();
            }
        }

        public void Update()
        {
            foreach (NetPlayer player in GetLoadedPlayers())
            {
                player.Update();
            }
        }

        public void LateUpdateFromState()
        {
            OnlineState state = client.State;
            ushort stageId = gameState.IsInGame ? (ushort)gameState.MainGameStage.stageIndex : ushort.MaxValue;

            //Send player info if the stage changed
            if (stageId != currentStageId)
            {
                currentStageId = stageId;
                objRoot = gameState.IsInGame ? GameObject.Find("ObjRoot").transform : null;
                localPlayer.SendPlayerInfo(state);
            }

            //Update snapshot
            if (gameState.IsInGame)
            {
                localPlayer.SendStagePosition(state);
            }

            //Remove players
            ushort[] playerIds = state.GetAllPlayers().ToArray();
            ushort[] removedPlayers = idToPlayer.Keys.Where(p => Array.IndexOf(playerIds, p) < 0).ToArray();
            foreach (ushort removedPlayerId in removedPlayers)
            {
                NetPlayer removedPlayer = idToPlayer[removedPlayerId];
                removedPlayer.Destroy();
                idToPlayer.Remove(removedPlayerId);
                OnPlayerDisconnected?.Invoke(this, new OnlinePlayerEventArgs(removedPlayer));
            }

            //Create new players
            ushort[] newPlayers = state.GetAllPlayers().Where(p => !idToPlayer.ContainsKey(p)).ToArray();
            foreach (ushort newPlayerId in newPlayers)
            {
                PlayerInfoRequest playerInfo = (PlayerInfoRequest)state.GetPlayerInfoType().GetPlayerData(newPlayerId);
                NetPlayer netPlayer = new NetPlayer(settings, newPlayerId, playerInfo.Name);
                netPlayer.AddNewPlayerInfo(playerInfo);
                idToPlayer.Add(newPlayerId, netPlayer);
                OnPlayerConnected?.Invoke(this, new OnlinePlayerEventArgs(netPlayer));
            }

            //Update players
            foreach (NetPlayer player in GetLoadedPlayers())
            {
                player.LateUpdateFromSnapshot((StagePositionSnapshot)state.GetStagePositionType().GetCurrentSnapshot(player.Id, client.Time));
            }
        }

        public void LateUpdateOutsideState()
        {
            foreach (NetPlayer player in GetAllPlayers())
            {
                player.LateUpdateOutsideState(currentStageId, gameState, objRoot);
            }
        }
    }
}
