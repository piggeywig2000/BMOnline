using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Server
{
    internal class RelaySnapshot
    {
        public RelaySnapshot(ushort relayId, uint tick, TimeSpan receivedOn, RelaySnapshotBroadcastType broadcastType, ushort broadcastTypeOperand, byte[] relayData)
        {
            RelayId = relayId;
            Tick = tick;
            ReceivedOn = receivedOn;
            BroadcastType = broadcastType;
            BroadcastTypeOperand = broadcastTypeOperand;
            RelayData = relayData;
        }

        public ushort RelayId { get; }
        public uint Tick { get; }
        public TimeSpan ReceivedOn { get; }
        public RelaySnapshotBroadcastType BroadcastType { get; }
        public ushort BroadcastTypeOperand { get; }
        public byte[] RelayData { get; }
    }
}
