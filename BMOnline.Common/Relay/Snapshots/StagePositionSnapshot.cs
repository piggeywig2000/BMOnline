using System;

namespace BMOnline.Common.Relay.Snapshots
{
    public class StagePositionSnapshot : ISnapshotPacket
    {
        public StagePositionSnapshot() : this((0, 0, 0), (0, 0, 0), 0, false) { }
        public StagePositionSnapshot((float, float, float) position, (float, float, float) angularVelocity, byte motionState, bool isOnGround) : this(position, (0, 0, 0), angularVelocity, motionState, isOnGround) { }
        public StagePositionSnapshot((float, float, float) position, (float, float, float) velocity, (float, float, float) angularVelocity, byte motionState, bool isOnGround)
        {
            Position = position;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            MotionState = motionState;
            IsOnGround = isOnGround;
        }

        public (float x, float y, float z) Position { get; private set; }
        public (float x, float y, float z) Velocity { get; private set; }
        public (float x, float y, float z) AngularVelocity { get; private set; }
        public byte MotionState { get; private set; }
        public bool IsOnGround { get; private set; }

        public void Decode(byte[] data)
        {
            Position = (BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8));
            AngularVelocity = (BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20));
            MotionState = (byte)(data[24] & 31);
            IsOnGround = (data[24] & 32) == 32;
        }

        public byte[] Encode()
        {
            byte[] output = new byte[25];
            BitConverter.GetBytes(Position.x).CopyTo(output, 0);
            BitConverter.GetBytes(Position.y).CopyTo(output, 4);
            BitConverter.GetBytes(Position.z).CopyTo(output, 8);
            BitConverter.GetBytes(AngularVelocity.x).CopyTo(output, 12);
            BitConverter.GetBytes(AngularVelocity.y).CopyTo(output, 16);
            BitConverter.GetBytes(AngularVelocity.z).CopyTo(output, 20);
            output[24] = (byte)(MotionState & 31 | (IsOnGround ? (byte)32 : (byte)0));
            return output;
        }

        public ISnapshotPacket LerpTo(ISnapshotPacket destination, float time, float gapSeconds)
        {
            StagePositionSnapshot to = (StagePositionSnapshot)destination;
            return new StagePositionSnapshot(
                (Position.x + (to.Position.x - Position.x) * time,
                Position.y + (to.Position.y - Position.y) * time,
                Position.z + (to.Position.z - Position.z) * time),
                ((to.Position.x - Position.x) / gapSeconds,
                (to.Position.y - Position.y) / gapSeconds,
                (to.Position.z - Position.z) / gapSeconds),
                (AngularVelocity.x + (to.AngularVelocity.x - AngularVelocity.x) * time,
                AngularVelocity.y + (to.AngularVelocity.y - AngularVelocity.y) * time,
                AngularVelocity.z + (to.AngularVelocity.z - AngularVelocity.z) * time),
                MotionState, IsOnGround);
        }

    }
}
