using System;
using System.Text;

namespace BMOnline.Common.Relay.Requests
{
    public class PlayerInfoRequest : IRelayPacket
    {
        public PlayerInfoRequest() : this("") { }
        public PlayerInfoRequest(string name) : this(name, byte.MaxValue, byte.MaxValue, ushort.MaxValue, 0, 0, new byte[10], new byte[10]) { }
        public PlayerInfoRequest(string name, byte mode, byte course, ushort stage, byte character, byte skinIndex, byte[] customisationsNum, byte[] customisationsChara)
        {
            Name = name;
            Mode = mode;
            Course = course;
            Stage = stage;
            Character = character;
            SkinIndex = skinIndex;
            CustomisationsNum = customisationsNum;
            CustomisationsChara = customisationsChara;
        }

        public string Name { get; private set; }
        public byte Mode { get; set; }
        public byte Course { get; set; }
        public ushort Stage { get; set; }
        public byte Character { get; private set; }
        public byte SkinIndex { get; private set; }
        public byte[] CustomisationsNum { get; private set; }
        public byte[] CustomisationsChara { get; private set; }

        public void Decode(byte[] data)
        {
            byte nameLength = data[0];
            Name = Encoding.UTF8.GetString(data, 1, nameLength);
            Mode = data[1 + nameLength];
            Course = data[1 + nameLength + 1];
            Stage = BitConverter.ToUInt16(data, 1 + nameLength + 2);
            Character = (byte)(data[1 + nameLength + 4] & 31);
            SkinIndex = (byte)(data[1 + nameLength + 4] >> 5);
            CustomisationsNum = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsNum[i] = data[1 + nameLength + 5 + i];
            }
            CustomisationsChara = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsChara[i] = data[1 + nameLength + 15 + i];
            }
        }

        public byte[] Encode()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(Name ?? "");
            byte[] output = new byte[1 + nameBytes.Length + 25];
            output[0] = (byte)nameBytes.Length;
            nameBytes.CopyTo(output, 1);
            output[1 + nameBytes.Length] = Mode;
            output[1 + nameBytes.Length + 1] = Course;
            BitConverter.GetBytes(Stage).CopyTo(output, 1 + nameBytes.Length + 2);
            output[1 + nameBytes.Length + 4] = (byte)((SkinIndex << 5) | (Character & 31));
            for (int i = 0; i < 10; i++)
            {
                output[1 + nameBytes.Length + 5 + i] = CustomisationsNum[i];
            }
            for (int i = 0; i < 10; i++)
            {
                output[1 + nameBytes.Length + 15 + i] = CustomisationsChara[i];
            }
            return output;
        }
    }
}
