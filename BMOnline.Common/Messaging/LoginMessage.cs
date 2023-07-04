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
        public ushort[] SnapshotIds { get; set; }
        public ushort[] RequestIds { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            ProtocolVersion = data[0];
            Secret = BitConverter.ToUInt32(data, 1);
            byte nameLength = data[5];
            Name = Encoding.UTF8.GetString(data, 6, nameLength);
            byte passwordLength = data[6 + nameLength];
            Password = Encoding.UTF8.GetString(data, 7 + nameLength, passwordLength);
            ushort snapshotIdLength = BitConverter.ToUInt16(data, 7 + nameLength + passwordLength);
            SnapshotIds = new ushort[snapshotIdLength];
            for (int i = 0; i < SnapshotIds.Length; i++)
            {
                SnapshotIds[i] = BitConverter.ToUInt16(data, (i * 2) + 9 + nameLength + passwordLength);
            }
            ushort requestIdLength = BitConverter.ToUInt16(data, 9 + nameLength + passwordLength + (snapshotIdLength * 2));
            RequestIds = new ushort[requestIdLength];
            for (int i = 0; i < RequestIds.Length; i++)
            {
                RequestIds[i] = BitConverter.ToUInt16(data, (i * 2) + 11 + nameLength + passwordLength + (snapshotIdLength * 2));
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(Name);
            byte[] passwordBytes = Password == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(Password);
            byte[] output = new byte[11 + nameBytes.Length + passwordBytes.Length + (SnapshotIds.Length * 2) + (RequestIds.Length * 2)];
            output[0] = ProtocolVersion;
            BitConverter.GetBytes(Secret).CopyTo(output, 1);
            output[5] = (byte)nameBytes.Length;
            nameBytes.CopyTo(output, 6);
            output[6 + nameBytes.Length] = (byte)passwordBytes.Length;
            passwordBytes.CopyTo(output, 7 + nameBytes.Length);
            BitConverter.GetBytes((ushort)SnapshotIds.Length).CopyTo(output, 7 + nameBytes.Length + passwordBytes.Length);
            for (int i = 0; i < SnapshotIds.Length; i++)
            {
                BitConverter.GetBytes(SnapshotIds[i]).CopyTo(output, (i * 2) + 9 + nameBytes.Length + passwordBytes.Length);
            }
            BitConverter.GetBytes((ushort)RequestIds.Length).CopyTo(output, 9 + nameBytes.Length + passwordBytes.Length + (SnapshotIds.Length * 2));
            for (int i = 0; i < RequestIds.Length; i++)
            {
                BitConverter.GetBytes(RequestIds[i]).CopyTo(output, (i * 2) + 11 + nameBytes.Length + passwordBytes.Length + (SnapshotIds.Length * 2));
            }
            return output;
        }
    }
}
