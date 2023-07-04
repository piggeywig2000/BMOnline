using System;

namespace BMOnline.Common.Relay.Requests
{
    [Obsolete]
    internal class RelayRequest : IRelayPacket
    {
        public RelayRequest(byte requestId, IRelayPacket data)
        {
            RequestId = requestId;
            Data = data;
        }

        public byte RequestId { get; private set; }
        public IRelayPacket Data { get; private set; }

        public void Decode(byte[] data)
        {
            RequestId = data[0];
            byte[] dataBytes = new byte[data.Length - 1];
            Array.Copy(data, 1, dataBytes, 0, dataBytes.Length);
            Data.Decode(dataBytes); 
        }

        public byte[] Encode()
        {
            byte[] databytes = Data.Encode();
            byte[] output = new byte[databytes.Length + 1];
            output[0] = RequestId;
            databytes.CopyTo(output, 1);
            return output;
        }
    }
}
