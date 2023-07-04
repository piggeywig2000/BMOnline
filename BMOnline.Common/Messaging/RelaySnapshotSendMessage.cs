using System;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Common.Messaging
{
    public class RelaySnapshotSendMessage : Message
    {
        public uint Secret { get; set; }
        public ushort RelayId { get; set; }
        public uint Tick { get; set; }
        public RelaySnapshotBroadcastType BroadcastType { get; set; }
        public ushort BroadcastTypeOperand { get; set; }
        public byte[] RelayData { get; set; }

        protected override void DecodeMessage(byte[] data)
        {
            Secret = BitConverter.ToUInt32(data, 0);
            RelayId = BitConverter.ToUInt16(data, 4);
            Tick = BitConverter.ToUInt32(data, 6);
            BroadcastType = (RelaySnapshotBroadcastType)data[10];
            BroadcastTypeOperand = BitConverter.ToUInt16(data, 11);
            ushort dataLength = BitConverter.ToUInt16(data, 13);
            RelayData = new byte[dataLength];
            Array.Copy(data, 15, RelayData, 0, dataLength);
        }

        protected override byte[] EncodeMessage()
        {
            byte[] output = new byte[15 + RelayData.Length];
            BitConverter.GetBytes(Secret).CopyTo(output, 0);
            BitConverter.GetBytes(RelayId).CopyTo(output, 4);
            BitConverter.GetBytes(Tick).CopyTo(output, 6);
            output[10] = (byte)BroadcastType;
            BitConverter.GetBytes(BroadcastTypeOperand).CopyTo(output, 11);
            BitConverter.GetBytes((ushort)RelayData.Length).CopyTo(output, 13);
            RelayData.CopyTo(output, 15);
            return output;
        }
    }
}
