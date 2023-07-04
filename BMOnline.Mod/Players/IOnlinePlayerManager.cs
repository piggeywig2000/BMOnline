using System;
using System.Collections.Generic;

namespace BMOnline.Mod.Players
{
    public interface IOnlinePlayerManager
    {
        /// <summary>
        /// Gets a player from its ID.
        /// </summary>
        /// <param name="id">The ID of the player to get.</param>
        /// <returns>The player, or null if there is no player with that ID.</returns>
        public IOnlinePlayer GetPlayer(ushort id);

        /// <summary>
        /// Gets all the players connected to the server, which may or may not be loaded into the current stage.
        /// </summary>
        /// <returns>An collection of all players connected to the server.</returns>
        public IReadOnlyCollection<IOnlinePlayer> GetAllPlayers();

        /// <summary>
        /// Gets all the players loaded into the current stage.
        /// </summary>
        /// <returns>A collection of all players loaded into the current stage.</returns>
        public IReadOnlyCollection<IOnlinePlayer> GetLoadedPlayers();

        /// <summary>
        /// Raised when a player connects to the server.
        /// </summary>
        public event EventHandler<OnlinePlayerEventArgs> OnPlayerConnected;

        /// <summary>
        /// Raised when a player disconnects from the server.
        /// </summary>
        public event EventHandler<OnlinePlayerEventArgs> OnPlayerDisconnected;
    }

    public class OnlinePlayerEventArgs : EventArgs
    {
        public OnlinePlayerEventArgs(IOnlinePlayer player)
        {
            Player = player;
        }

        public IOnlinePlayer Player { get; }
    }
}
