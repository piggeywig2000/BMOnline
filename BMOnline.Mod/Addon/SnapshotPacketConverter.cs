namespace BMOnline.Mod.Addon
{
    internal class SnapshotPacketConverter : Common.Relay.Snapshots.ISnapshotPacket
    {
        public readonly IAddonSnapshotPacket underlyingPacket;

        public SnapshotPacketConverter(IAddonSnapshotPacket underlyingPacket)
        {
            this.underlyingPacket = underlyingPacket;
        }

        public void Decode(byte[] data) => underlyingPacket.Decode(data);

        public byte[] Encode() => underlyingPacket.Encode();

        public Common.Relay.Snapshots.ISnapshotPacket LerpTo(Common.Relay.Snapshots.ISnapshotPacket destination, float time, float gapSeconds)
            => new SnapshotPacketConverter(underlyingPacket.LerpTo(((SnapshotPacketConverter)destination).underlyingPacket, time, gapSeconds)); //This is so cursed
    }
}
