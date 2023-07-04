namespace BMOnline.Common.Relay.Snapshots
{
    public interface ISnapshotPacket : IRelayPacket
    {
        /// <summary>
        /// Returns a new snapshot representing the linear interpolation from the current snapshot to the provided snapshot.
        /// </summary>
        /// <param name="destination">The other snapshot to interpolate to.</param>
        /// <param name="time">The current time, between 0 and 1, where 0 is this snapshot and 1 is the destination snapshot.</param>
        /// <param name="gapSeconds">The total time difference between the two snapshots in seconds.</param>
        /// <returns></returns>
        ISnapshotPacket LerpTo(ISnapshotPacket destination, float time, float gapSeconds);
    }
}
