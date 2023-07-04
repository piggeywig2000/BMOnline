namespace BMOnline.Mod.Addon
{
    internal class RequestPacketConverter : Common.Relay.IRelayPacket
    {
        public readonly IAddonRequestPacket underlyingPacket;

        public RequestPacketConverter(IAddonRequestPacket underlyingPacket)
        {
            this.underlyingPacket = underlyingPacket;
        }

        public void Decode(byte[] data) => underlyingPacket.Decode(data);

        public byte[] Encode() => underlyingPacket.Encode();
    }
}
