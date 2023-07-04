using BMOnline.Client;
using BMOnline.Client.Relay.Snapshots;

namespace BMOnline.Mod.Addon
{
    internal class AddonSnapshotType : IAddonSnapshotType
    {
        private OnlineClient client;

        public AddonSnapshotType(RelaySnapshotType snapshotType)
        {
            SnapshotType = snapshotType;
        }

        public RelaySnapshotType SnapshotType { get; }

        public void Initialise(OnlineClient client)
        {
            this.client = client;
        }

        public IAddonSnapshotPacket GetCurrentSnapshot(ushort playerId)
        {
            client.ThrowIfNotInStateSemaphore();
            return ((SnapshotPacketConverter)SnapshotType.GetCurrentSnapshot(playerId, client.Time))?.underlyingPacket;
        }

        public void SetSnapshotToSend(IAddonSnapshotPacket packet, SnapshotBroadcastType broadcastType, ushort broadcastTypeOperand)
        {
            client.ThrowIfNotInStateSemaphore();
            SnapshotType.SetSnapshotToSend(new SnapshotPacketConverter(packet), (Common.Relay.Snapshots.RelaySnapshotBroadcastType)broadcastType, broadcastTypeOperand);
        }
    }
}
