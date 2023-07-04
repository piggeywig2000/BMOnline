using System;

namespace BMOnline.Mod.Addon
{
    public interface IAddonRequestType
    {
        /// <summary>
        /// Raised when a player is updated for this relay request type.
        /// </summary>
        public event EventHandler<AddonRequestPlayerEventArgs> OnPlayerUpdated;

        /// <summary>
        /// Get the latest packet received from the given player.
        /// </summary>
        /// <param name="playerId">The ID of the player to get the latest packet of.</param>
        /// <returns>The latest packet received from the player.</returns>
        public IAddonRequestPacket GetPlayerData(ushort playerId);

        /// <summary>
        /// Send a new packet to all players. Do not call this method if the contents of the data have not changed since the last call to this method.
        /// </summary>
        /// <param name="packet">The new packet to send out.</param>
        public void SendData(IAddonRequestPacket packet);
    }

    public class AddonRequestPlayerEventArgs : EventArgs
    {
        public AddonRequestPlayerEventArgs(ushort playerId, IAddonRequestPacket data)
        {
            PlayerId = playerId;
            Data = data;
        }

        public ushort PlayerId { get; }
        public IAddonRequestPacket Data { get; }
    }
}
