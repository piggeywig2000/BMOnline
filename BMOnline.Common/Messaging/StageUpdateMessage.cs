using System;

namespace BMOnline.Common.Messaging
{
    public class StageUpdateMessage : Message
    {
        public class StagePlayer : Message
        {
            public ushort Id { get; set; }
            public uint Tick { get; set; }
            public ushort AgeMs { get; set; }
            public (float, float, float) Positon { get; set; }
            public (float, float, float) AngularVelocity { get; set; }
            public byte MotionState { get; set; }

            protected override void DecodeMessage(byte[] data)
            {
                Id = BitConverter.ToUInt16(data, 0);
                Tick = BitConverter.ToUInt32(data, 2);
                AgeMs = BitConverter.ToUInt16(data, 6);
                Positon = (BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16));
                AngularVelocity = (BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24), BitConverter.ToSingle(data, 28));
                MotionState = data[32];
            }

            protected override byte[] EncodeMessage()
            {
                byte[] output = new byte[33];
                BitConverter.GetBytes(Id).CopyTo(output, 0);
                BitConverter.GetBytes(Tick).CopyTo(output, 2);
                BitConverter.GetBytes(AgeMs).CopyTo(output, 6);
                BitConverter.GetBytes(Positon.Item1).CopyTo(output, 8);
                BitConverter.GetBytes(Positon.Item2).CopyTo(output, 12);
                BitConverter.GetBytes(Positon.Item3).CopyTo(output, 16);
                BitConverter.GetBytes(AngularVelocity.Item1).CopyTo(output, 20);
                BitConverter.GetBytes(AngularVelocity.Item2).CopyTo(output, 24);
                BitConverter.GetBytes(AngularVelocity.Item3).CopyTo(output, 28);
                output[32] = MotionState;
                return output;
            }
        }

        public uint Secret { get; set; }
        public ushort Stage { get; set; }
        public StagePlayer[] Players { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            Stage = BitConverter.ToUInt16(data, 4);
            Players = new StagePlayer[data[6]];
            int pointer = 7;
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new StagePlayer();
                byte[] playerData = new byte[33];
                Array.Copy(data, pointer, playerData, 0, playerData.Length);
                pointer += playerData.Length;
                Message.DecodeRaw(playerData, Players[i]);
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[7 + (Players.Length * 33)];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(Stage).CopyTo(output, 4);
            output[6] = (byte)Math.Min(Players.Length, byte.MaxValue);
            int pointer = 7;
            for (int i = 0; i < Players.Length; i++)
            {
                byte[] playerData = Players[i].EncodeRaw();
                playerData.CopyTo(output, pointer);
                pointer += playerData.Length;
            }
            return output;
        }
    }
}
