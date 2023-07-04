using System;

namespace BMOnline.Common.Messaging
{
    public class RelayRequestResponseMessage : Message
    {
        public uint Secret { get; set; }
        public ushort RelayId { get; set; }
        public ushort PlayerId { get; set; }
        public byte RequestId { get; set; }
        public byte[] RelayData { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            RelayId = BitConverter.ToUInt16(data, 4);
            PlayerId = BitConverter.ToUInt16(data, 6);
            RequestId = data[8];
            ushort dataLength = BitConverter.ToUInt16(data, 9);
            RelayData = new byte[dataLength];
            Array.Copy(data, 11, RelayData, 0, dataLength);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[11 + RelayData.Length];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(RelayId).CopyTo(output, 4);
            BitConverter.GetBytes(PlayerId).CopyTo(output, 6);
            output[8] = RequestId;
            BitConverter.GetBytes((ushort)RelayData.Length).CopyTo(output, 9);
            RelayData.CopyTo(output, 11);
            return output;
        }
    }
}
