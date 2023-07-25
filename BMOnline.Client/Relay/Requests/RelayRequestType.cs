using System;
using System.Collections.Generic;
using System.Linq;
using BMOnline.Common;
using BMOnline.Common.Messaging;
using BMOnline.Common.Relay;

namespace BMOnline.Client.Relay.Requests
{
    public class RelayRequestType
    {
        private readonly Dictionary<ushort, (ushort requestId, IRelayPacket requestData)> playerToReceivedRequest;
        private readonly Dictionary<ushort, byte> playerToLatestRequestId;
        private readonly HashSet<ushort> updatedPlayers;
        private readonly Func<IRelayPacket> packetConstructor;

        private byte clientSideRequestId;
        private byte serverSideRequestId;

        public class RelayRequestPlayerEventArgs : EventArgs
        {
            public RelayRequestPlayerEventArgs(ushort playerId, IRelayPacket data)
            {
                PlayerId = playerId;
                Data = data;
            }

            public ushort PlayerId { get; }
            public IRelayPacket Data { get; }
        }

        public event EventHandler<RelayRequestPlayerEventArgs> OnPlayerUpdated;

        public RelayRequestType(ushort relayTypeId, Func<IRelayPacket> packetConstructor)
        {
            playerToReceivedRequest = new Dictionary<ushort, (ushort, IRelayPacket)>();
            playerToLatestRequestId = new Dictionary<ushort, byte>();
            updatedPlayers = new HashSet<ushort>();
            this.packetConstructor = packetConstructor;
            RelayTypeId = relayTypeId;
            DataToSend = null;
            clientSideRequestId = byte.MaxValue;
            serverSideRequestId = byte.MaxValue;
        }

        public ushort RelayTypeId { get; }
        public IRelayPacket DataToSend { get; private set; }

        public void ClearLatestRequestIds()
        {
            playerToLatestRequestId.Clear();
            serverSideRequestId = byte.MaxValue;
        }

        public void UpdatePlayerRequestId(ushort playerId, byte newRequestId) => playerToLatestRequestId[playerId] = newRequestId;

        public void UpdateServerSideRequestId(byte newRequestId) => serverSideRequestId = newRequestId;

        public IReadOnlyCollection<ushort> GetAllPlayers() => playerToReceivedRequest.Keys;

        public IEnumerable<ushort> GetPlayersToRequest()
            => playerToLatestRequestId.Keys.Where(playerId => !playerToReceivedRequest.ContainsKey(playerId) || playerToLatestRequestId[playerId] != playerToReceivedRequest[playerId].requestId);

        public void AddResponse(RelayRequestResponseMessage responseMessage)
        {
            if (RelayTypeId != responseMessage.RelayId)
                throw new InvalidOperationException("Tried to add a request relay response with the incorrect relay type ID");
            if (!playerToLatestRequestId.ContainsKey(responseMessage.PlayerId))
                return;
            IRelayPacket response = packetConstructor();
            try
            {
                response.Decode(responseMessage.RelayData);
                if (!playerToReceivedRequest.ContainsKey(responseMessage.PlayerId) || playerToReceivedRequest[responseMessage.PlayerId].requestId != responseMessage.RequestId)
                    updatedPlayers.Add(responseMessage.PlayerId);
                playerToReceivedRequest[responseMessage.PlayerId] = (responseMessage.RequestId, response);
            }
            catch (Exception e)
            {
                Log.Warning($"Exception while decoding relay request packet with type ID {responseMessage.RelayId}: {e}");
            }
        }

        public IRelayPacket GetPlayerData(ushort playerId)
            => playerToReceivedRequest.ContainsKey(playerId) ? playerToReceivedRequest[playerId].requestData : null;

        public void RaiseUpdatedEvents()
        {
            foreach (ushort playerId in updatedPlayers)
            {
                OnPlayerUpdated?.Invoke(this, new RelayRequestPlayerEventArgs(playerId, playerToReceivedRequest[playerId].requestData));
            }
            updatedPlayers.Clear();
        }

        public void RemovePlayer(ushort playerId)
        {
            playerToReceivedRequest.Remove(playerId);
            playerToLatestRequestId.Remove(playerId);
            updatedPlayers.Remove(playerId);
        }

        public void SendData(IRelayPacket data)
        {
            clientSideRequestId++;
            DataToSend = data;
        }

        public (byte requestId, IRelayPacket data) GetDataToSend()
            => clientSideRequestId != serverSideRequestId && DataToSend != null ? (clientSideRequestId, DataToSend) : (byte.MaxValue, null);
    }
}
