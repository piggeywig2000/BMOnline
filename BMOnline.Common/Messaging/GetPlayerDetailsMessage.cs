using System;

namespace BMOnline.Common.Messaging
{
    public class GetPlayerDetailsMessage : Message
    {
        public uint Secret { get; set; }
        public ushort[] RequestedPlayers { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            byte length = data[4];
            RequestedPlayers = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                RequestedPlayers[i] = BitConverter.ToUInt16(data, 5 + (i * 2));
            }
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[5 + (RequestedPlayers.Length * 2)];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            output[4] = (byte)Math.Min(RequestedPlayers.Length, byte.MaxValue);
            for (int i = 0; i <  RequestedPlayers.Length; i++)
            {
                BitConverter.GetBytes(RequestedPlayers[i]).CopyTo(output, 5 + (i * 2));
            }
            return output;
        }
    }
}
