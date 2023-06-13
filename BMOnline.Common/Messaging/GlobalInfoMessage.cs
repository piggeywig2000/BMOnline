using System;

namespace BMOnline.Common.Messaging
{
    public class GlobalInfoMessage : Message
    {
        public uint Secret { get; set; }
        public ushort OnlineCount { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            OnlineCount = BitConverter.ToUInt16(data, 4);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[6];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(OnlineCount).CopyTo(output, 4);
            return output;
        }
    }
}
