using System;

namespace BMOnline.Common.Messaging
{
    public abstract class StatusMessage : Message
    {
        public uint Secret { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[4];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            return output;
        }
    }
}
