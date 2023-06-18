using System;

namespace BMOnline.Common.Messaging
{
    public class GlobalInfoMessage : Message
    {
        public uint Secret { get; set; }
        public ushort OnlineCount { get; set; }
        public byte LatestChatIndex { get; set; }
        public byte RequestedChatIndex { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            OnlineCount = BitConverter.ToUInt16(data, 4);
            LatestChatIndex = data[6];
            RequestedChatIndex = data[7];
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[8];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(OnlineCount).CopyTo(output, 4);
            output[6] = LatestChatIndex;
            output[7] = RequestedChatIndex;
            return output;
        }
    }
}
