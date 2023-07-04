using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BMOnline.Client.Relay.Requests;
using BMOnline.Client.Relay.Snapshots;
using BMOnline.Common;
using BMOnline.Common.Messaging;
using BMOnline.Common.Relay;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Client
{
    public class OnlineClient : BidirectionalUdp
    {
        private static readonly Random secretGenerator = new Random();

        private readonly IPEndPoint serverEndpoint;
        private readonly OnlineState state;
        private TimeSpan lastPacketReceived = TimeSpan.FromSeconds(-30);
        private bool isLoggedIn = true;
        private readonly string password;
        private uint secret = 0;

        public OnlineClient(IPAddress ip, ushort port, string name, string password, RelaySnapshotType[] snapshotTypes, RelayRequestType[] requestTypes) : base(new IPEndPoint(IPAddress.Any, 0))
        {
            serverEndpoint = new IPEndPoint(ip, port);
            state = new OnlineState(this, snapshotTypes, requestTypes);
            Name = name;
            this.password = password;
        }
        public OnlineClient(IPAddress ip, ushort port, string name, RelaySnapshotType[] snapshotTypes, RelayRequestType[] requestTypes) : this(ip, port, name, null, snapshotTypes, requestTypes) { }

        public string Name { get; }
        public bool IsConnected => isLoggedIn && lastPacketReceived > TimeSpan.Zero;
        public LoginRefuseReason? RefuseReason { get; private set; } = null;
        public OnlineState State
        {
            get
            {
                ThrowIfNotInStateSemaphore();
                return state;
            }
        }
        public SemaphoreSlim StateSemaphore { get; } = new SemaphoreSlim(1);

        public void ThrowIfNotInStateSemaphore()
        {
            if (StateSemaphore.CurrentCount > 0)
                throw new InvalidOperationException("Attempted to access the OnlineClient's State from outside the semaphore.");
        }

        protected override async Task HandleReceive(TimedUdpReceive result)
        {
            Message message = Message.Decode(result.Buffer);
            if (message is LoginRefuseMessage loginRefuseMessage)
            {
                HandleLoginRefuse(loginRefuseMessage);
                return;
            }
            else if (message is GlobalInfoMessage globalInfoMessage)
            {
                await HandleGlobalInfo(globalInfoMessage);
            }
            else if (message is ChatMessage chatMessage)
            {
                await HandleChat(chatMessage);
            }
            else if (message is RelaySnapshotReceiveMessage snapshotReceiveMessage)
            {
                await HandleSnapshotReceiveMessage(snapshotReceiveMessage, result.TimeReceived);
            }
            else if (message is RelayRequestPlayersMessage requestPlayersMessage)
            {
                await HandleRequestPlayersMessage(requestPlayersMessage);
            }
            else if (message is RelayRequestResponseMessage requestResponseMessage)
            {
                await HandleRequestResponseMessage(requestResponseMessage);
            }
            else
            {
                return;
            }
            lastPacketReceived = Time;
            RefuseReason = null;

            if (!isLoggedIn)
            {
                Log.Success($"Logged in as {Name}");
                isLoggedIn = true;
            }
        }

        private void HandleLoginRefuse(LoginRefuseMessage message)
        {
            if (message.Secret != secret) return;

            RefuseReason = message.Reason;
        }

        private async Task HandleGlobalInfo(GlobalInfoMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            State.OnlineCount = message.OnlineCount;
            State.MaxChatLength = message.MaxChatLength;
            if (State.IncomingChats == null)
            {
                State.OutgoingChats = new Common.Chat.OutgoingChatBuffer();
                State.IncomingChats = new Common.Chat.IncomingChatBuffer(message.LatestChatIndex);
            }
            State.OutgoingChats.SetRequestedIndex(message.RequestedChatIndex);
            StateSemaphore.Release();
        }

        private async Task HandleChat(ChatMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            State.IncomingChats?.ReceiveChatFromRemote(message.Index, message.Content);
            StateSemaphore.Release();
        }

        private async Task HandleSnapshotReceiveMessage(RelaySnapshotReceiveMessage message, TimeSpan timeReceived)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            State.GetRelaySnapshotType(message.RelayId)?.AddSnapshot(message, timeReceived);
            StateSemaphore.Release();
        }

        private async Task HandleRequestPlayersMessage(RelayRequestPlayersMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            //Remove non-existant players
            ushort[] removedPlayers = State.GetAllPlayers().Where(playerId => message.Players.All(player => player.Id != playerId)).ToArray();
            foreach (ushort removedPlayer in removedPlayers)
            {
                foreach (RelaySnapshotType snapshotType in State.GetAllRelaySnapshotTypes())
                    snapshotType.RemovePlayer(removedPlayer);
                foreach (RelayRequestType requestType in State.GetAllRelayRequestTypes())
                    requestType.RemovePlayer(removedPlayer);
            }
            //Update request IDs on all relay request types
            foreach (RelayRequestType requestType in State.GetAllRelayRequestTypes())
            {
                requestType.ClearLatestRequestIds();
            }
            foreach (RelayRequestPlayersMessage.RelayRequestPlayer player in message.Players)
            {
                for (int i = 0; i < player.RelayIds.Length; i++)
                {
                    RelayRequestType requestType = State.GetRelayRequestType(player.RelayIds[i]);
                    if (requestType != null)
                    {
                        if (player.Id == message.ClientPlayerId)
                            requestType.UpdateServerSideRequestId(player.RequestIds[i]);
                        else
                            requestType.UpdatePlayerRequestId(player.Id, player.RequestIds[i]);
                    }
                }
            }
            StateSemaphore.Release();
        }

        private async Task HandleRequestResponseMessage(RelayRequestResponseMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            State.GetRelayRequestType(message.RelayId)?.AddResponse(message);
            StateSemaphore.Release();
        }

        protected override async Task SendTick()
        {
            await StateSemaphore.WaitAsync();

            //Check if we need to log in
            if (isLoggedIn && Time - lastPacketReceived > TimeSpan.FromSeconds(5))
            {
                if (!IsConnected)
                    Log.Info($"Logging in as {Name}");
                else
                    Log.Warning("Connection timed out, logging back in");
                isLoggedIn = false;
                secret = (uint)secretGenerator.Next();
                //Reset chat buffers
                State.OutgoingChats = null;
                State.IncomingChats = null;
            }

            //If we're not logged in, send a login message
            if (!isLoggedIn)
            {
                LoginMessage loginMessage = new LoginMessage()
                {
                    ProtocolVersion = PROTOCOL_VERSION,
                    Secret = secret,
                    Name = Name,
                    Password = password,
                    SnapshotIds = State.GetAllRelaySnapshotTypes().Select(t => t.RelayTypeId).ToArray(),
                    RequestIds = State.GetAllRelayRequestTypes().Select(t => t.RelayTypeId).ToArray()
                };
                await SendAsync(loginMessage.Encode(), serverEndpoint);
            }
            else
            {
                //Send status message
                byte[] statusBytes = new StatusMessage()
                {
                    Secret = secret,
                    RequestedChatIndex = State.IncomingChats?.IndexToRequest ?? 0
                }.Encode();
                await SendAsync(statusBytes, serverEndpoint);
                //Send a chat message
                string outgoingChat = State.OutgoingChats?.GetChatAtIndex(State.OutgoingChats.RequestedIndex);
                if (outgoingChat != null)
                {
                    byte[] chatBytes = new ChatMessage()
                    {
                        Secret = secret,
                        Index = State.OutgoingChats.RequestedIndex,
                        Content = outgoingChat
                    }.Encode();
                    await SendAsync(chatBytes, serverEndpoint);
                }
                //Send snapshots
                foreach (RelaySnapshotType snapshotType in State.GetAllRelaySnapshotTypes())
                {
                    (ISnapshotPacket snapshotToSend, RelaySnapshotBroadcastType broadcastType, ushort broadcastTypeOperand) = snapshotType.GetSnapshotToSend();
                    if (snapshotToSend != null)
                    {
                        byte[] snapshotBytes = new RelaySnapshotSendMessage()
                        {
                            Secret = secret,
                            RelayId = snapshotType.RelayTypeId,
                            Tick = CurrentTick,
                            BroadcastType = broadcastType,
                            BroadcastTypeOperand = broadcastTypeOperand,
                            RelayData = snapshotToSend.Encode()
                        }.Encode();
                        await SendAsync(snapshotBytes, serverEndpoint);
                    }
                }
                //Send requests
                foreach (RelayRequestType requestType in State.GetAllRelayRequestTypes())
                {
                    ushort[] playersToRequest = requestType.GetPlayersToRequest().ToArray();
                    if (playersToRequest.Length > 0)
                    {
                        byte[] playerRequestBytes = new RelayRequestGetMessage()
                        {
                            Secret = secret,
                            RelayId = requestType.RelayTypeId,
                            RequestedPlayers = playersToRequest
                        }.Encode();
                        await SendAsync(playerRequestBytes, serverEndpoint);
                    }
                    (byte requestId, IRelayPacket data) = requestType.GetDataToSend();
                    if (data != null)
                    {
                        byte[] requestUpdatebytes = new RelayRequestUpdateMessage()
                        {
                            Secret = secret,
                            RelayId = requestType.RelayTypeId,
                            RequestId = requestId,
                            RelayData = data.Encode()
                        }.Encode();
                        await SendAsync(requestUpdatebytes, serverEndpoint);
                    }
                }
            }

            StateSemaphore.Release();
        }
    }
}
