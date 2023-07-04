using System;

namespace BMOnline.Common.Messaging
{
    public class StatusMessage : Message
    {
        public uint Secret { get; set; }
        public byte RequestedChatIndex { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            RequestedChatIndex = data[4];
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[5];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            output[4] = RequestedChatIndex;
            return output;
        }
    }
}
