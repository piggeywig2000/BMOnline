using System;
using BMOnline.Client;
using BMOnline.Client.Relay.Requests;

namespace BMOnline.Mod.Addon
{
    internal class AddonRequestType : IAddonRequestType
    {
        private OnlineClient client;

        public AddonRequestType(RelayRequestType requestType)
        {
            RequestType = requestType;

            RequestType.OnPlayerUpdated += (s, e) => OnPlayerUpdated?.Invoke(this, new AddonRequestPlayerEventArgs(e.PlayerId, ((RequestPacketConverter)e.Data).underlyingPacket));
        }

        public RelayRequestType RequestType { get; }

        public event EventHandler<AddonRequestPlayerEventArgs> OnPlayerUpdated;

        public void Initialise(OnlineClient client)
        {
            this.client = client;
        }

        public IAddonRequestPacket GetPlayerData(ushort playerId)
        {
            client.ThrowIfNotInStateSemaphore();
            return ((RequestPacketConverter)RequestType.GetPlayerData(playerId))?.underlyingPacket;
        }

        public void SendData(IAddonRequestPacket packet)
        {
            client.ThrowIfNotInStateSemaphore();
            RequestType.SendData(new RequestPacketConverter(packet));
        }
    }
}
