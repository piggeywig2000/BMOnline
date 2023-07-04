using System;

namespace BMOnline.Common.Messaging
{
    public class RelayRequestGetMessage : Message
    {
        public uint Secret { get; set; }
        public ushort RelayId { get; set; }
        public ushort[] RequestedPlayers { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            RelayId = BitConverter.ToUInt16(data, 4);
            ushort length = BitConverter.ToUInt16(data, 6);
            RequestedPlayers = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                RequestedPlayers[i] = BitConverter.ToUInt16(data, 8 + (i * 2));
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[8 + (RequestedPlayers.Length * 2)];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(RelayId).CopyTo(output, 4);
            BitConverter.GetBytes((ushort)RequestedPlayers.Length).CopyTo(output, 6);
            for (int i = 0; i < RequestedPlayers.Length; i++)
            {
                BitConverter.GetBytes(RequestedPlayers[i]).CopyTo(output, 8 + (i * 2));
            }
            return output;
        }
    }
}
