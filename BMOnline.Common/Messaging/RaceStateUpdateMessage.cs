using System;

namespace BMOnline.Common.Messaging
{
    public class RaceStateUpdateMessage : Message
    {
        public uint Secret { get; set; }
        public ushort Stage { get; set; }
        public bool IsLoaded { get; set; }
        public float FinishTime { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            Stage = BitConverter.ToUInt16(data, 4);
            IsLoaded = data[6] != 0;
            FinishTime = BitConverter.ToSingle(data, 7);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[11];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(Stage).CopyTo(output, 4);
            output[6] = IsLoaded ? (byte)1 : (byte)0;
            BitConverter.GetBytes(FinishTime).CopyTo(output, 7);
            return output;
        }
    }
}
