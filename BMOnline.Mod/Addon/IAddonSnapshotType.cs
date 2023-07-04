namespace BMOnline.Mod.Addon
{
    public interface IAddonSnapshotType
    {
        /// <summary>
        /// Gets a packet representing the state from a few snapshots ago, interpolated between the actual snapshots that were received to create smooth transitions.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public IAddonSnapshotPacket GetCurrentSnapshot(ushort playerId);

        /// <summary>
        /// Sets the packet that should be sent. The particular packet passed in may or may not actually be sent, depending on timing. This method should be called as often as possible, preferably every frame.
        /// </summary>
        /// <param name="packet">The new packet that should be sent.</param>
        /// <param name="broadcastType">Who should receive this snapshot. All players, all players in a particular stage, or a particular player.</param>
        /// <param name="broadcastTypeOperand">Depends on the broadcast type. If it's everyone, this is ignored. If it's everyone in stage, this value should be the ID of the stage. If it's particular player, this value should be the ID of that player.</param>
        public void SetSnapshotToSend(IAddonSnapshotPacket packet, SnapshotBroadcastType broadcastType, ushort broadcastTypeOperand);
    }
}
