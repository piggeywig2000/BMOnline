using System;
using System.Collections.Generic;
using BMOnline.Common;

namespace BMOnline.Client
{
    public class OnlinePlayer
    {
        private const int MAX_SNAPSHOTS = 10;
        private const double TICK_DELAY = 4; //We have a delay of 4 ticks before showing the position, to allow the packets some time to arrive
        private readonly OnlineClient onlineClient;
        private readonly SortedList<uint, OnlinePlayerSnapshot> snapshots;

        public OnlinePlayer(ushort id, OnlineClient onlineClient)
        {
            Id = id;
            HasDetails = false;
            this.onlineClient = onlineClient;
            snapshots = new SortedList<uint, OnlinePlayerSnapshot>(MAX_SNAPSHOTS);
        }

        public ushort Id { get; }
        public bool HasDetails { get; private set; }
        public string Name { get; private set; }
        public byte Character { get; private set; }
        public byte SkinIndex { get; private set; }
        public byte[] CustomisationsNum { get; private set; }
        public byte[] CustomisationsChara { get; private set; }

        public void SetDetails(string name, byte character, byte skinIndex, byte[] customisationsNum, byte[] customisationsChara)
        {
            HasDetails = true;
            Name = name;
            Character = character;
            SkinIndex = skinIndex;
            CustomisationsNum = customisationsNum;
            CustomisationsChara = customisationsChara;
        }

        public void AddSnapshot(uint tick, OnlinePosition position, byte motionState, bool isOnGround, TimeSpan snapshotTime)
        {
            if (snapshots.ContainsKey(tick)) return; //Don't add if snapshot already exists (due to duplicate packet)
            if (snapshots.Count == MAX_SNAPSHOTS) snapshots.RemoveAt(0);
            snapshots.Add(tick, new OnlinePlayerSnapshot(tick, snapshotTime, position, motionState, isOnGround));
        }

        private OnlinePlayerSnapshot GetLatestSnapshot() => snapshots.Values[snapshots.Count - 1];

        /// <summary>
        /// Gets the time elapsed between now and the latest snapshot.
        /// Note that this time is not equivalent to just subtracting the two DateTimes,
        /// because this function takes an average of all the snapshot times to stop the time jumping when new snapshots are added.
        /// </summary>
        /// <returns>The rough time elapsed between now and the latest snapshot.</returns>
        private TimeSpan TimeSinceLatestSnapshot()
        {
            if (snapshots.Count == 0) throw new InvalidOperationException("Trying to get time since latest snapshot with no snapshots");

            TimeSpan now = onlineClient.Time;
            OnlinePlayerSnapshot latest = GetLatestSnapshot();

            double sum = 0;
            double total = 0;
            for (int i = 0; i < snapshots.Count; i++)
            {
                TimeSpan gap = now - snapshots.Values[i].SnapshotTime;
                gap -= OnlineClient.TICK_LENGTH.Multiply(latest.Tick - snapshots.Values[i].Tick);

                //The idea for the weight is to stop snapshots that just came in or are just about to be removed from having as much of an impact,
                //so that when a snapshot is added/removed the time doesn't suddenly jump.
                double weight;
                if (snapshots.Count <= 2)
                    weight = 1;
                else
                    weight = (-Math.Cos(i * (Math.PI / (snapshots.Count - 1)) * 2) / 2) + 0.5;

                sum += gap.TotalMilliseconds * weight;
                total += weight;
            }

            //If total is 0, it means there was one snapshot and its value was 0, meaning it was literally just created
            return total > 0 ? TimeSpan.FromMilliseconds(sum / total) : TimeSpan.Zero;
        }

        public OnlinePosition GetLatestPosition() => GetLatestSnapshot().Position;

        private OnlinePlayerSnapshot? GetSnapshotBefore(TimeSpan targetTime, uint latestTick, TimeSpan timeSinceLatest)
        {
            OnlinePlayerSnapshot? before = null;
            foreach (OnlinePlayerSnapshot snapshot in snapshots.Values)
            {
                TimeSpan snapshotTime = OnlineClient.TICK_LENGTH.Multiply(latestTick - snapshot.Tick) + timeSinceLatest;
                if (snapshotTime >= targetTime && (!before.HasValue || before.Value.Tick < snapshot.Tick))
                {
                    before = snapshot;
                }
            }
            return before;
        }

        private OnlinePlayerSnapshot? GetSnapshotAfter(TimeSpan targetTime, uint latestTick, TimeSpan timeSinceLatest)
        {
            OnlinePlayerSnapshot? after = null;
            foreach (OnlinePlayerSnapshot snapshot in snapshots.Values)
            {
                TimeSpan snapshotTime = OnlineClient.TICK_LENGTH.Multiply(latestTick - snapshot.Tick) + timeSinceLatest;
                if (snapshotTime < targetTime && (!after.HasValue || after.Value.Tick > snapshot.Tick))
                {
                    after = snapshot;
                }
            }
            return after;
        }

        /// <summary>
        /// Estimates the current position by interpolating between two snapshots
        /// </summary>
        /// <returns></returns>
        public OnlinePosition? GetPosition()
        {
            //Figure out the time since the latest snapshot
            uint latestTick = GetLatestSnapshot().Tick;
            TimeSpan timeSinceLatest = TimeSinceLatestSnapshot();
            TimeSpan targetTime = OnlineClient.TICK_LENGTH.Multiply(TICK_DELAY);

            //Get the snapshot immediately before and immediately after the target time
            OnlinePlayerSnapshot? before = GetSnapshotBefore(targetTime, latestTick, timeSinceLatest);
            OnlinePlayerSnapshot? after = GetSnapshotAfter(targetTime, latestTick, timeSinceLatest);

            if (!before.HasValue || !after.HasValue)
            {
                //The time falls out of the range of our snapshots, so we can't interpolate between two of them
                return null;
            }
            else
            {
                //Interpolate the position between two snapshots
                TimeSpan beforeTime = OnlineClient.TICK_LENGTH.Multiply(latestTick - before.Value.Tick) + timeSinceLatest;
                TimeSpan afterTime = OnlineClient.TICK_LENGTH.Multiply(latestTick - after.Value.Tick) + timeSinceLatest;
                double t = (beforeTime - targetTime).Divide(beforeTime - afterTime);
                return OnlinePosition.Lerp(before.Value.Position, after.Value.Position, (float)t);
            }
        }

        public (float, float, float) GetVelocity()
        {
            //Figure out the time since the latest snapshot
            uint latestTick = GetLatestSnapshot().Tick;
            TimeSpan timeSinceLatest = TimeSinceLatestSnapshot();
            TimeSpan targetTime = OnlineClient.TICK_LENGTH.Multiply(TICK_DELAY);

            //Get the snapshot immediately before and immediately after the target time
            OnlinePlayerSnapshot? before = GetSnapshotBefore(targetTime, latestTick, timeSinceLatest);
            OnlinePlayerSnapshot? after = GetSnapshotAfter(targetTime, latestTick, timeSinceLatest);

            if (!before.HasValue || !after.HasValue)
            {
                return (0, 0, 0);
            }
            else
            {
                float t = (float)OnlineClient.TICK_LENGTH.Multiply(after.Value.Tick - before.Value.Tick).TotalSeconds;
                float x = (after.Value.Position.PosX - before.Value.Position.PosX) / t;
                float y = (after.Value.Position.PosY - before.Value.Position.PosY) / t;
                float z = (after.Value.Position.PosZ - before.Value.Position.PosZ) / t;
                return (x, y, z);
            }
        }

        public byte? GetMotionState()
        {
            uint latestTick = GetLatestSnapshot().Tick;
            TimeSpan timeSinceLatest = TimeSinceLatestSnapshot();
            TimeSpan targetTime = OnlineClient.TICK_LENGTH.Multiply(TICK_DELAY);

            OnlinePlayerSnapshot? before = GetSnapshotBefore(targetTime, latestTick, timeSinceLatest);
            return before?.MotionState;
        }

        public bool? GetIsOnGround()
        {
            uint latestTick = GetLatestSnapshot().Tick;
            TimeSpan timeSinceLatest = TimeSinceLatestSnapshot();
            TimeSpan targetTime = OnlineClient.TICK_LENGTH.Multiply(TICK_DELAY);

            OnlinePlayerSnapshot? before = GetSnapshotBefore(targetTime, latestTick, timeSinceLatest);
            return before?.IsOnGround;
        }
    }
}
