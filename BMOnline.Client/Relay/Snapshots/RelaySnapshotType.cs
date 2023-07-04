using System;
using System.Collections.Generic;
using BMOnline.Common.Messaging;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Client.Relay.Snapshots
{
    public class RelaySnapshotType
    {
        private readonly Dictionary<ushort, RelaySnapshotSet> playerToReceivedSet;
        private readonly Func<ISnapshotPacket> snapshotConstructor;

        private RelaySnapshotBroadcastType broadcastType;
        private ushort broadcastTypeOperand;

        public RelaySnapshotType(ushort relayTypeId, Func<ISnapshotPacket> snapshotConstructor)
        {
            playerToReceivedSet = new Dictionary<ushort, RelaySnapshotSet>();
            this.snapshotConstructor = snapshotConstructor;
            RelayTypeId = relayTypeId;
            SnapshotToSend = null;
            broadcastType = RelaySnapshotBroadcastType.EveryoneOnServer;
            broadcastTypeOperand = 0;
        }

        public ushort RelayTypeId { get; }
        public ISnapshotPacket SnapshotToSend { get; private set; }

        public void AddSnapshot(RelaySnapshotReceiveMessage snapshot, TimeSpan timeReceived)
        {
            if (snapshot.RelayId != RelayTypeId)
                throw new InvalidOperationException("Attempted to add a snapshot using a message with a difference relay ID");
            foreach (RelaySnapshotReceiveMessage.RelaySnapshotPlayer playerSnapshot in snapshot.Players)
            {
                if (!playerToReceivedSet.TryGetValue(playerSnapshot.Id, out RelaySnapshotSet snapshotSet))
                {
                    snapshotSet = new RelaySnapshotSet(snapshotConstructor);
                    playerToReceivedSet.Add(playerSnapshot.Id, snapshotSet);
                }
                snapshotSet.AddSnapshot(playerSnapshot, timeReceived);
            }
        }

        public ISnapshotPacket GetCurrentSnapshot(ushort playerId, TimeSpan now) => playerToReceivedSet.TryGetValue(playerId, out RelaySnapshotSet snapshotSet) ? snapshotSet.GetCurrentSnapshot(now) : null;

        public ISnapshotPacket GetLatestSnapshot(ushort playerId) => playerToReceivedSet.TryGetValue(playerId, out RelaySnapshotSet snapshotSet) ? snapshotSet.GetLatestSnapshot() : null;

        public void ClearPlayerSnapshots(ushort playerId)
        {
            if (playerToReceivedSet.TryGetValue(playerId, out RelaySnapshotSet set))
                set.ClearSnapshots();
        }

        public void RemovePlayer(ushort playerId)
        {
            playerToReceivedSet.Remove(playerId);
        }

        public void SetSnapshotToSend(ISnapshotPacket snapshotToSend, RelaySnapshotBroadcastType broadcastType, ushort broadcastTypeOperand)
        {
            SnapshotToSend = snapshotToSend;
            this.broadcastType = broadcastType;
            this.broadcastTypeOperand = broadcastTypeOperand;
        }

        public (ISnapshotPacket snapshotToSend, RelaySnapshotBroadcastType broadcastType, ushort broadcastTypeOperand) GetSnapshotToSend()
        {
            ISnapshotPacket snapshotToSend = SnapshotToSend;
            RelaySnapshotBroadcastType broadcastType = this.broadcastType;
            ushort broadcastTypeOperand = this.broadcastTypeOperand;
            SnapshotToSend = null;
            this.broadcastType = RelaySnapshotBroadcastType.EveryoneOnServer;
            this.broadcastTypeOperand = 0;
            return (snapshotToSend, broadcastType, broadcastTypeOperand);
        }
    }
}
