using System;

namespace BMOnline.Common.Messaging
{
    public class RelayRequestUpdateMessage : Message
    {
        public uint Secret { get; set; }
        public ushort RelayId { get; set; }
        public byte RequestId { get; set; }
        public byte[] RelayData { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            RelayId = BitConverter.ToUInt16(data, 4);
            RequestId = data[6];
            ushort dataLength = BitConverter.ToUInt16(data, 7);
            RelayData = new byte[dataLength];
            Array.Copy(data, 9, RelayData, 0, dataLength);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[9 + RelayData.Length];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(RelayId).CopyTo(output, 4);
            output[6] = RequestId;
            BitConverter.GetBytes((ushort)RelayData.Length).CopyTo(output, 7);
            RelayData.CopyTo(output, 9);
            return output;
        }
    }
}
