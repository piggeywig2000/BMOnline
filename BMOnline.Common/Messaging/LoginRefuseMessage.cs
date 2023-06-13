using System;

namespace BMOnline.Common.Messaging
{
    public class LoginRefuseMessage : Message
    {
        public uint Secret { get; set; }
        public LoginRefuseReason Reason { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            Reason = (LoginRefuseReason)data[4];
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[5];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            output[4] = (byte)Reason;
            return output;
        }
    }
}
