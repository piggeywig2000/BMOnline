using System;
using System.Linq;

namespace BMOnline.Common.Messaging
{
    public class RelayRequestPlayersMessage : Message
    {
        public class RelayRequestPlayer : Message
        {
            public ushort Id { get; set; }
            public ushort[] RelayIds { get; set; }
            public byte[] RequestIds { get; set; }

            protected override void DecodeMessage(byte[] data)
            {
                Id = BitConverter.ToUInt16(data, 0);
                ushort relayIdLength = BitConverter.ToUInt16(data, 2);
                RelayIds = new ushort[relayIdLength];
                for (int i = 0; i < RelayIds.Length; i++)
                {
                    RelayIds[i] = BitConverter.ToUInt16(data, 4 + (i * 2));
                }
                ushort requestIdLength = BitConverter.ToUInt16(data, 4 + (relayIdLength * 2));
                RequestIds = new byte[requestIdLength];
                for (int i = 0; i < RequestIds.Length; i++)
                {
                    RequestIds[i] = data[6 + (relayIdLength * 2) + i];
                }
            }

            protected override byte[] EncodeMessage()
            {
                byte[] output = new byte[6 + (RelayIds.Length * 2) + RequestIds.Length];
                BitConverter.GetBytes(Id).CopyTo(output, 0);
                BitConverter.GetBytes((ushort)RelayIds.Length).CopyTo(output, 2);
                for (int i = 0; i < RelayIds.Length; i++)
                {
                    BitConverter.GetBytes(RelayIds[i]).CopyTo(output, 4 + (i * 2));
                }
                BitConverter.GetBytes((ushort)RequestIds.Length).CopyTo(output, 4 + (RelayIds.Length * 2));
                for (int i = 0; i < RequestIds.Length; i++)
                {
                    output[6 + (RelayIds.Length * 2) + i] = RequestIds[i];
                }
                return output;
            }
        }

        public uint Secret { get; set; }
        public ushort ClientPlayerId { get; set; }
        public RelayRequestPlayer[] Players { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            ClientPlayerId = BitConverter.ToUInt16(data, 4);
            ushort playerLength = BitConverter.ToUInt16(data, 6);
            Players = new RelayRequestPlayer[playerLength];
            int pointer = 8;
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i] = new RelayRequestPlayer();
                ushort relayLength = BitConverter.ToUInt16(data, pointer + 2);
                ushort requestLength = BitConverter.ToUInt16(data, pointer + (relayLength * 2) + 4);
                byte[] playerBytes = new byte[6 + (relayLength * 2) + requestLength];
                Array.Copy(data, pointer, playerBytes, 0, playerBytes.Length);
                pointer += playerBytes.Length;
                Message.DecodeRaw(playerBytes, Players[i]);
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[8 + Players.Sum(p => 6 + (p.RelayIds.Length * 2) + p.RequestIds.Length)];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(ClientPlayerId).CopyTo(output, 4);
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
