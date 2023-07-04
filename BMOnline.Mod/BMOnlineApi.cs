using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using BMOnline.Client;
using BMOnline.Client.Relay.Requests;
using BMOnline.Client.Relay.Snapshots;
using BMOnline.Common;
using BMOnline.Mod.Addon;
using BMOnline.Mod.Chat;
using BMOnline.Mod.Notifications;
using BMOnline.Mod.Players;
using BMOnline.Mod.Settings;
using Flash2;

namespace BMOnline.Mod
{
    internal class BMOnlineApi : IBMOnlineApi
    {
        private OnlineClient client;
        private Task clientLoop;
        private GameState gameState;
        private ConnectStateManager connectStateManager;
        private PlayerCountManager playerCountManager;

        private bool hasShownWelcomeChat = false;

        private readonly List<AddonSnapshotType> snapshotTypes = new List<AddonSnapshotType>();
        private readonly List<AddonRequestType> requestTypes = new List<AddonRequestType>();

        public event EventHandler DoStateUpdate;

        public BMOnlineApi(Dictionary<string, object> settingsDict)
        {
            settings = new BmoSettings(settingsDict);
            IsInitialised = false;
            IsConnected = false;
            IsFatalErrored = false;
        }

        private readonly BmoSettings settings;
        public IBmoSettings Settings => settings;

        private NetPlayerManager playerManager;
        public IOnlinePlayerManager PlayerManager => playerManager;

        private NotificationManager notificationManager;
        public INotificationManager NotificationManager => notificationManager;

        private ChatManager chatManager;
        public IChatManager ChatManager => chatManager;

        public bool IsInitialised { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsFatalErrored { get; private set; }

        public void Initialise(string name)
        {
            if (IsInitialised)
                throw new InvalidOperationException("Tried to initialise when the API is already initialised");
            if (string.IsNullOrWhiteSpace(name))
                name = "Player";
            else if (name.Length > 32)
                name = name.Substring(0, 32);

            gameState = new GameState();

            connectStateManager = new ConnectStateManager();

            try
            {
                client = new OnlineClient(settings.ServerIpAddress.Value, settings.ServerPort.Value, name, settings.ServerPassword.Value, snapshotTypes.Select(type => type.SnapshotType).ToArray(), requestTypes.Select(type => type.RequestType).ToArray());
            }
            catch (SocketException e)
            {
                Log.Error($"Failed to connect to the server. The server is not running or unreachable. Exception details:\n{e}");
                connectStateManager.SetDisconnected("Could not connect to server");
                IsFatalErrored = true;
                return;
            }
            catch (Exception e)
            {
                Log.Error($"An unknown error occurred while connecting to the server. Exception details:\n{e}");
                connectStateManager.SetDisconnected("Could not connect to server");
                IsFatalErrored = true;
                return;
            }

            clientLoop = Task.Run(client.RunBusy);

            playerCountManager = new PlayerCountManager(Settings);
            playerManager = new NetPlayerManager(settings, gameState, client);
            notificationManager = new NotificationManager(Settings);
            chatManager = new ChatManager(Settings);

            foreach (AddonSnapshotType snapshotType in snapshotTypes)
            {
                snapshotType.Initialise(client);
            }
            foreach (AddonRequestType requestType in requestTypes)
            {
                requestType.Initialise(client);
            }

            IsInitialised = true;
        }

        public void FixedUpdate()
        {
            if (!IsInitialised)
                return;

            playerManager.FixedUpdate();
        }

        public void Update()
        {
            if (!IsInitialised)
                return;

            playerManager.Update();
        }

        public void LateUpdate()
        {
            if (!IsInitialised)
                return;

            settings.Update();
            notificationManager.Update();

            if (IsFatalErrored) return;

            if (client.RefuseReason != null)
            {
                switch (client.RefuseReason)
                {
                    case LoginRefuseReason.ProtocolVersion:
                        connectStateManager.SetDisconnected("Wrong mod version\nMake sure the mod is on the latest version");
                        break;
                    case LoginRefuseReason.Password:
                        connectStateManager.SetDisconnected("Incorrect password");
                        break;
                    case LoginRefuseReason.BadName:
                        connectStateManager.SetDisconnected("Invalid name");
                        break;
                }
                return;
            }

            //Ensure networking client is still running
            if (clientLoop.IsCompleted)
            {
                Log.Error($"An unknown networking error occurred. Exception details:\n{clientLoop.Exception}");
                connectStateManager.SetDisconnected("Could not connect to server");
                IsFatalErrored = true;
                return;
            }

            if (!gameState.IsInGame)
                playerCountManager.RecreatePlayerCountsIfNeeded();

            if (!client.IsConnected)
                connectStateManager.SetConnecting();

            connectStateManager.SetVisibility(!gameState.IsInGame || Pause.isEnable);

            if (client.IsConnected && !hasShownWelcomeChat)
            {
                chatManager.AddChatMessage("Welcome to Banana Mania Online!\nPress 'T' to type in the chat.\nPress F1 to see keybinds.");
                hasShownWelcomeChat = true;
            }
            string outgoingMessage = chatManager.UpdateAndGetSubmittedChat();

            if (client.StateSemaphore.Wait(0))
            {
                try
                {
                    if (client.IsConnected)
                        connectStateManager.SetConnected(client.State.OnlineCount);

                    playerManager.LateUpdateFromState();

                    foreach (RelayRequestType requestType in client.State.GetAllRelayRequestTypes())
                    {
                        requestType.RaiseUpdatedEvents();
                    }

                    if (!gameState.IsInGame)
                        playerCountManager.UpdatePlayerCounts(playerManager.GetAllPlayers(), client.CurrentTick);

                    if (chatManager.MaxChatLength != client.State.MaxChatLength)
                        chatManager.MaxChatLength = client.State.MaxChatLength;
                    if (outgoingMessage != null && client.State.OutgoingChats != null)
                        client.State.OutgoingChats.SendChat(outgoingMessage);
                    if (client.State.IncomingChats != null && client.State.IncomingChats.HasReceivedChat)
                    {
                        string incomingMessage = client.State.IncomingChats.GetReceivedChat();
                        chatManager.AddChatMessage(incomingMessage);
                    }

                    DoStateUpdate?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    client.StateSemaphore.Release();
                }
            }

            playerManager.LateUpdateOutsideState();

            gameState.ClearFlags();
        }

        public IAddonSnapshotType RegisterRelaySnapshotType(byte addonId, byte typeId, Func<IAddonSnapshotPacket> packetConstructor)
        {
            if (IsInitialised)
                throw new InvalidOperationException("Tried to register a relay packet after initialisation. Relay packets should be registered in OnModLoad");
            if (addonId == 0)
                throw new ArgumentException("Addon ID cannot be 0", nameof(addonId));
            AddonSnapshotType snapshotType = new AddonSnapshotType(new RelaySnapshotType((ushort)((addonId << 8) + typeId), () => new SnapshotPacketConverter(packetConstructor())));
            snapshotTypes.Add(snapshotType);
            return snapshotType;
        }

        public IAddonRequestType RegisterRelayRequestType(byte addonId, byte typeId, Func<IAddonRequestPacket> packetConstructor)
        {
            if (IsInitialised)
                throw new InvalidOperationException("Tried to register a relay packet after initialisation. Relay packets should be registered in OnModLoad");
            if (addonId == 0)
                throw new ArgumentException("Addon ID cannot be 0", nameof(addonId));
            AddonRequestType requestType = new AddonRequestType(new RelayRequestType((ushort)((addonId << 8) + typeId), () => new RequestPacketConverter(packetConstructor())));
            requestTypes.Add(requestType);
            return requestType;
        }
    }
}
