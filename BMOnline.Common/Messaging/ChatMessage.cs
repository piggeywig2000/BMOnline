using System;
using System.Collections.Generic;
using System.Text;

namespace BMOnline.Common.Messaging
{
    public class ChatMessage : Message
    {
        public uint Secret { get; set; }
        public byte Index { get; set; }
        public string Content { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            Index = data[4];
            ushort contentLength = BitConverter.ToUInt16(data, 5);
            Content = Encoding.UTF8.GetString(data, 7, contentLength);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(Content);
            byte[] output = new byte[7 + contentBytes.Length];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            output[4] = Index;
            BitConverter.GetBytes((ushort)contentBytes.Length).CopyTo(output, 5);
            contentBytes.CopyTo(output, 7);
            return output;
        }
    }
}
