using System;

namespace BMOnline.Common.Messaging
{
    public class GameStatusMessage : StatusMessage
    {
        public byte Course { get; set; }
        public ushort Stage { get; set; }
        public uint Tick { get; set; }
        public (float, float, float) Position { get; set; }
        public (float, float, float) AngularVelocity { get; set; }
        public byte MotionState { get; set; }
        public byte Character { get; set; }
        public byte[] CustomisationsNum { get; set; }
        public byte[] CustomisationsChara { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            base.DecodeMessage(data);
            Course = data[4];
            Stage = BitConverter.ToUInt16(data, 5);
            Tick = BitConverter.ToUInt32(data, 7);
            Position = (BitConverter.ToSingle(data, 11), BitConverter.ToSingle(data, 15), BitConverter.ToSingle(data, 19));
            AngularVelocity = (BitConverter.ToSingle(data, 23), BitConverter.ToSingle(data, 27), BitConverter.ToSingle(data, 31));
            MotionState = data[35];
            Character = data[36];
            CustomisationsNum = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsNum[i] = data[37 + i];
            }
            CustomisationsChara = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsChara[i] = data[47 + i];
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[57];
            base.EncodeMessage().CopyTo(output, 0);
            output[4] = Course;
            BitConverter.GetBytes(Stage).CopyTo(output, 5);
            BitConverter.GetBytes(Tick).CopyTo(output, 7);
            BitConverter.GetBytes(Position.Item1).CopyTo(output, 11);
            BitConverter.GetBytes(Position.Item2).CopyTo(output, 15);
            BitConverter.GetBytes(Position.Item3).CopyTo(output, 19);
            BitConverter.GetBytes(AngularVelocity.Item1).CopyTo(output, 23);
            BitConverter.GetBytes(AngularVelocity.Item2).CopyTo(output, 27);
            BitConverter.GetBytes(AngularVelocity.Item3).CopyTo(output, 31);
            output[35] = MotionState;
            output[36] = Character;
            for (int i = 0; i < 10; i++)
            {
                output[37 + i] = CustomisationsNum[i];
            }
            for (int i = 0; i < 10; i++)
            {
                output[47 + i] = CustomisationsChara[i];
            }
            return output;
        }
    }
}
