using System;
using System.Collections.Generic;
using BMOnline.Common;
using BMOnline.Common.Messaging;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Client.Relay.Snapshots
{
    public class RelaySnapshotSet
    {
        readonly struct SnapshotEntry
        {
            public readonly uint Tick;
            public readonly TimeSpan TimeReceived;
            public readonly ISnapshotPacket Snapshot;

            public SnapshotEntry(uint tick, TimeSpan timeReceived, ISnapshotPacket snapshot)
            {
                Tick = tick;
                TimeReceived = timeReceived;
                Snapshot = snapshot;
            }
        }

        private const int MAX_SNAPSHOTS = 50;
        private const double TICK_DELAY = 4; //We have a delay of 4 ticks before showing the position, to allow the packets some time to arrive
        private readonly SortedList<uint, SnapshotEntry> snapshots;
        private readonly Func<ISnapshotPacket> snapshotConstructor;
        private readonly ushort relayTypeId;

        public RelaySnapshotSet(ushort relayTypeId, Func<ISnapshotPacket> snapshotConstructor)
        {
            snapshots = new SortedList<uint, SnapshotEntry>();
            this.snapshotConstructor = snapshotConstructor;
            this.relayTypeId = relayTypeId;
        }

        public void AddSnapshot(RelaySnapshotReceiveMessage.RelaySnapshotPlayer snapshot, TimeSpan timeReceived)
        {
            if (snapshots.ContainsKey(snapshot.Tick)) return; //Don't add if snapshot already exists (due to duplicate packet)
            if (snapshots.Count == MAX_SNAPSHOTS) snapshots.RemoveAt(0);
            ISnapshotPacket snapshotData = snapshotConstructor();
            try
            {
                snapshotData.Decode(snapshot.RelayData);
                snapshots.Add(snapshot.Tick, new SnapshotEntry(snapshot.Tick, timeReceived - TimeSpan.FromMilliseconds(snapshot.AgeMs), snapshotData));
            }
            catch (Exception e)
            {
                Log.Warning($"Exception while decoding relay snapshot packet with type ID {relayTypeId}: {e}");
            }
        }

        /// <summary>
        /// Gets the time elapsed between now and the latest snapshot.
        /// Note that this time is not how long ago the latest snapshot was received,
        /// because this function takes an average of all the snapshot times to stop the time jumping when new snapshots are added.
        /// </summary>
        /// <returns>The rough time elapsed between now and the latest snapshot.</returns>
        private TimeSpan TimeSinceLatestSnapshot(TimeSpan now)
        {
            if (snapshots.Count == 0) throw new InvalidOperationException("Trying to get time since latest snapshot with no snapshots");

            SnapshotEntry latest = snapshots.Values[snapshots.Count - 1];

            double sum = 0;
            double total = 0;
            for (int i = 0; i < snapshots.Count; i++)
            {
                TimeSpan gap = now - snapshots.Values[i].TimeReceived;
                gap -= BidirectionalUdp.TICK_LENGTH.Multiply(latest.Tick - snapshots.Values[i].Tick);

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

        private SnapshotEntry? GetSnapshotBefore(TimeSpan targetTime, uint latestTick, TimeSpan timeSinceLatest)
        {
            SnapshotEntry? before = null;
            foreach (SnapshotEntry snapshot in snapshots.Values)
            {
                TimeSpan snapshotTime = BidirectionalUdp.TICK_LENGTH.Multiply(latestTick - snapshot.Tick) + timeSinceLatest;
                if (snapshotTime >= targetTime && (!before.HasValue || before.Value.Tick < snapshot.Tick))
                {
                    before = snapshot;
                }
            }
            return before;
        }

        private SnapshotEntry? GetSnapshotAfter(TimeSpan targetTime, uint latestTick, TimeSpan timeSinceLatest)
        {
            SnapshotEntry? after = null;
            foreach (SnapshotEntry snapshot in snapshots.Values)
            {
                TimeSpan snapshotTime = BidirectionalUdp.TICK_LENGTH.Multiply(latestTick - snapshot.Tick) + timeSinceLatest;
                if (snapshotTime < targetTime && (!after.HasValue || after.Value.Tick > snapshot.Tick))
                {
                    after = snapshot;
                }
            }
            return after;
        }

        public ISnapshotPacket GetCurrentSnapshot(TimeSpan now)
        {
            if (snapshots.Count == 0)
                return null;

            //Figure out the time since the latest snapshot
            uint latestTick = snapshots.Values[snapshots.Count - 1].Tick;
            TimeSpan timeSinceLatest = TimeSinceLatestSnapshot(now);
            TimeSpan targetTime = BidirectionalUdp.TICK_LENGTH.Multiply(TICK_DELAY);

            //Get the snapshot immediately before and immediately after the target time
            SnapshotEntry? before = GetSnapshotBefore(targetTime, latestTick, timeSinceLatest);
            SnapshotEntry? after = GetSnapshotAfter(targetTime, latestTick, timeSinceLatest);

            if (!before.HasValue && !after.HasValue)
            {
                //No snapshots, return null
                return null;
            }
            else if (!before.HasValue)
            {
                //Current time is before all of our snapshots, return first snapshot
                return snapshots.Values[0].Snapshot;
            }
            else if (!after.HasValue)
            {
                //Current time is after all of our snapshots, return last snapshot
                return snapshots.Values[snapshots.Count - 1].Snapshot;
            }

            //Interpolate the position between two snapshots
            TimeSpan beforeTime = BidirectionalUdp.TICK_LENGTH.Multiply(latestTick - before.Value.Tick) + timeSinceLatest;
            TimeSpan afterTime = BidirectionalUdp.TICK_LENGTH.Multiply(latestTick - after.Value.Tick) + timeSinceLatest;
            float t = (float)(beforeTime - targetTime).Divide(beforeTime - afterTime);
            float gap = (float)BidirectionalUdp.TICK_LENGTH.Multiply(after.Value.Tick - before.Value.Tick).TotalSeconds;
            return before.Value.Snapshot.LerpTo(after.Value.Snapshot, t, gap);
        }

        public ISnapshotPacket GetLatestSnapshot() => snapshots.Count > 0 ? snapshots.Values[snapshots.Count - 1].Snapshot : null;

        public void ClearSnapshots() => snapshots.Clear();
    }
}
