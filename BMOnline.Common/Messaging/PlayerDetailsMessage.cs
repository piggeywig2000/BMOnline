using System;
using System.Text;

namespace BMOnline.Common.Messaging
{
    public class PlayerDetailsMessage : Message
    {
        public uint Secret { get; set; }
        public ushort PlayerId { get; set; }
        public string Name { get; set; }
        public byte Character { get; set; }
        public byte[] CustomisationsNum { get; set; }
        public byte[] CustomisationsChara { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            PlayerId = BitConverter.ToUInt16(data, 4);
            byte nameLength = data[6];
            Name = Encoding.UTF8.GetString(data, 7, nameLength);
            Character = data[7 + nameLength];
            CustomisationsNum = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsNum[i] = data[nameLength + 8 + i];
            }
            CustomisationsChara = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsChara[i] = data[nameLength + 18 + i];
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(Name);
            byte[] output = new byte[nameBytes.Length + 28];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(PlayerId).CopyTo(output, 4);
            output[6] = (byte)nameBytes.Length;
            nameBytes.CopyTo(output, 7);
            output[7 + nameBytes.Length] = Character;
            for (int i = 0; i < 10; i++)
            {
                output[nameBytes.Length + 8 + i] = CustomisationsNum[i];
            }
            for (int i = 0; i < 10; i++)
            {
                output[nameBytes.Length + 18 + i] = CustomisationsChara[i];
            }
            return output;
        }
    }
}
