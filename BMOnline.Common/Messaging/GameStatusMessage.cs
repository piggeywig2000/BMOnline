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
            int baseLength = data.Length - 53;
            Course = data[baseLength];
            Stage = BitConverter.ToUInt16(data, baseLength + 1);
            Tick = BitConverter.ToUInt32(data, baseLength + 3);
            Position = (BitConverter.ToSingle(data, baseLength + 7), BitConverter.ToSingle(data, baseLength + 11), BitConverter.ToSingle(data, baseLength + 15));
            AngularVelocity = (BitConverter.ToSingle(data, baseLength + 19), BitConverter.ToSingle(data, baseLength + 23), BitConverter.ToSingle(data, baseLength + 27));
            MotionState = data[baseLength + 31];
            Character = data[baseLength + 32];
            CustomisationsNum = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsNum[i] = data[baseLength + 33 + i];
            }
            CustomisationsChara = new byte[10];
            for (int i = 0; i < 10; i++)
            {
                CustomisationsChara[i] = data[baseLength + 43 + i];
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] baseData = base.EncodeMessage();
            byte[] output = new byte[baseData.Length + 53];
            baseData.CopyTo(output, 0);
            output[baseData.Length] = Course;
            BitConverter.GetBytes(Stage).CopyTo(output, baseData.Length + 1);
            BitConverter.GetBytes(Tick).CopyTo(output, baseData.Length + 3);
            BitConverter.GetBytes(Position.Item1).CopyTo(output, baseData.Length + 7);
            BitConverter.GetBytes(Position.Item2).CopyTo(output, baseData.Length + 11);
            BitConverter.GetBytes(Position.Item3).CopyTo(output, baseData.Length + 15);
            BitConverter.GetBytes(AngularVelocity.Item1).CopyTo(output, baseData.Length + 19);
            BitConverter.GetBytes(AngularVelocity.Item2).CopyTo(output, baseData.Length + 23);
            BitConverter.GetBytes(AngularVelocity.Item3).CopyTo(output, baseData.Length + 27);
            output[baseData.Length + 31] = MotionState;
            output[baseData.Length + 32] = Character;
            for (int i = 0; i < 10; i++)
            {
                output[baseData.Length + 33 + i] = CustomisationsNum[i];
            }
            for (int i = 0; i < 10; i++)
            {
                output[baseData.Length + 43 + i] = CustomisationsChara[i];
            }
            return output;
        }
    }
}
