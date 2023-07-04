using System;
using BMOnline.Mod.Addon;
using BMOnline.Mod.Chat;
using BMOnline.Mod.Notifications;
using BMOnline.Mod.Players;
using BMOnline.Mod.Settings;

namespace BMOnline.Mod
{
    public interface IBMOnlineApi
    {
        public IBmoSettings Settings { get; }
        public IOnlinePlayerManager PlayerManager { get; }
        public INotificationManager NotificationManager { get; }
        public IChatManager ChatManager { get; }

        public bool IsInitialised { get; }
        public bool IsConnected { get; }
        public bool IsFatalErrored { get; }

        /// <summary>
        /// Raised once per frame. Event handlers can access and modify the networking state. Occasionally will not be raised on a frame.
        /// </summary>
        public event EventHandler DoStateUpdate;
        
        /// <summary>
        /// Registers a new snapshot packet type. Snapshot packets constantly send updates to other players.
        /// Packets can be interpolated to make numerical values change smoothly, making it ideal for things like sending the current position.
        /// </summary>
        /// <param name="addonId">The addon ID. Should be the same across all packet types in your addon. Should be unique to your addon, don't use other addon's IDs.</param>
        /// <param name="typeId">The packet type ID. Each packet type in your addon should have a unique value.</param>
        /// <param name="packetConstructor">Returns a new packet with default values. This is used to construct the packet before the Decode method is called.</param>
        /// <returns>The snapshot packet type registered.</returns>
        public IAddonSnapshotType RegisterRelaySnapshotType(byte addonId, byte typeId, Func<IAddonSnapshotPacket> packetConstructor);

        /// <summary>
        /// Registers a new request packet type. Request packets send updates only when a value changes.
        /// Do not use request packets if the packet data constantly changes, because it's very inefficient. Use snapshot packets instead.
        /// </summary>
        /// <param name="addonId">The addon ID. Should be the same across all packet types in your addon. Should be unique to your addon, don't use other addon's IDs.</param>
        /// <param name="typeId">The packet type ID. Each packet type in your addon should have a unique value.</param>
        /// <param name="packetConstructor">Returns a new packet with default values. This is used to construct the packet before the Decode method is called.</param>
        /// <returns>The request packet type registered.</returns>
        public IAddonRequestType RegisterRelayRequestType(byte addonId, byte typeId, Func<IAddonRequestPacket> packetConstructor);
    }
}
