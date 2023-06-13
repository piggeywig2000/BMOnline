using System;
using System.Text;

namespace BMOnline.Common.Messaging
{
    public class LoginMessage : Message
    {
        public byte ProtocolVersion { get; set; }
        public uint Secret { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            ProtocolVersion = data[0];
            Secret = BitConverter.ToUInt32(data, 1);
            byte nameLength = data[5];
            Name = Encoding.UTF8.GetString(data, 6, nameLength);
            byte passwordLength = data[6 + nameLength];
            Password = Encoding.UTF8.GetString(data, 7 + nameLength, passwordLength);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(Name);
            byte[] passwordBytes = Password == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(Password);
            byte[] output = new byte[7 + nameBytes.Length + passwordBytes.Length];
            output[0] = ProtocolVersion;
            BitConverter.GetBytes(Secret).CopyTo(output, 1);
            output[5] = (byte)nameBytes.Length;
            nameBytes.CopyTo(output, 6);
            output[6 + nameBytes.Length] = (byte)passwordBytes.Length;
            passwordBytes.CopyTo(output, 7 + nameBytes.Length);
            return output;
        }
    }
}
