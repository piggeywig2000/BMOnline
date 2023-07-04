using System;
using System.Linq;

namespace BMOnline.Common.Messaging
{
    public class RelaySnapshotReceiveMessage : Message
    {
        public class RelaySnapshotPlayer : Message
        {
            public ushort Id { get; set; }
            public uint Tick { get; set; }
            public ushort AgeMs { get; set; }
            public byte[] RelayData { get; set; }

            protected override void DecodeMessage(byte[] data)
            {
                Id = BitConverter.ToUInt16(data, 0);
                Tick = BitConverter.ToUInt32(data, 2);
                AgeMs = BitConverter.ToUInt16(data, 6);
                ushort dataLength = BitConverter.ToUInt16(data, 8);
                RelayData = new byte[dataLength];
                Array.Copy(data, 10, RelayData, 0, dataLength);
            }

            protected override byte[] EncodeMessage()
            {
                ushort dataLength = (ushort)RelayData.Length;
                byte[] output = new byte[10 + dataLength];
                BitConverter.GetBytes(Id).CopyTo(output, 0);
                BitConverter.GetBytes(Tick).CopyTo(output, 2);
                BitConverter.GetBytes(AgeMs).CopyTo(output, 6);
                BitConverter.GetBytes(dataLength).CopyTo(output, 8);
                RelayData.CopyTo(output, 10);
                return output;
            }
        }

        public uint Secret { get; set; }
        public ushort RelayId { get; set; }
        public RelaySnapshotPlayer[] Players { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            RelayId = BitConverter.ToUInt16(data, 4);
            ushort playerLength = BitConverter.ToUInt16(data, 6);
            Players = new RelaySnapshotPlayer[playerLength];
            int pointer = 8;
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new RelaySnapshotPlayer();
                byte[] playerData = new byte[10 + BitConverter.ToUInt16(data, pointer + 8)];
                Array.Copy(data, pointer, playerData, 0, playerData.Length);
                pointer += playerData.Length;
                Message.DecodeRaw(playerData, Players[i]);
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[8 + Players.Sum(p => 10 + p.RelayData.Length)];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(RelayId).CopyTo(output, 4);
            BitConverter.GetBytes((ushort)Players.Length).CopyTo(output, 6);
            int pointer = 8;
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
