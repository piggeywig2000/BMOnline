namespace BMOnline.Server
{
    internal class RelayRequest
    {
        public RelayRequest(ushort relayId, byte requestId, byte[] relayData)
        {
            RelayId = relayId;
            RequestId = requestId;
            RelayData = relayData;
        }

        public ushort RelayId { get; }
        public byte RequestId { get; }
        public byte[] RelayData { get; }
    }
}
