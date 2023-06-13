using System;

namespace BMOnline.Client
{
    internal readonly struct OnlinePlayerSnapshot
    {
        public readonly uint Tick;
        public readonly TimeSpan SnapshotTime;
        public readonly OnlinePosition Position;
        public readonly byte MotionState;
        public readonly bool IsOnGround;

        public OnlinePlayerSnapshot(uint tick, TimeSpan snapshotTime, OnlinePosition position, byte motionState, bool isOnGround)
        {
            Tick = tick;
            SnapshotTime = snapshotTime;
            Position = position;
            MotionState = motionState;
            IsOnGround = isOnGround;
        }
    }
}
