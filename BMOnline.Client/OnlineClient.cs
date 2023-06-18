using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BMOnline.Common;
using BMOnline.Common.Messaging;

namespace BMOnline.Client
{
    public class OnlineClient : BidirectionalUdp
    {
        private static readonly Random secretGenerator = new Random();

        private readonly IPEndPoint serverEndpoint;
        private TimeSpan lastPacketReceived = TimeSpan.FromSeconds(-30);
        private bool isLoggedIn = true;
        private readonly string password;
        private uint secret = 0;

        public OnlineClient(IPAddress ip, ushort port, string name, string password) : base(new IPEndPoint(IPAddress.Any, 0))
        {
            serverEndpoint = new IPEndPoint(ip, port);
            State = new OnlineState(this);
            Name = name;
            this.password = password;
        }
        public OnlineClient(IPAddress ip, ushort port, string name) : this(ip, port, name, null) { }

        public string Name { get; }
        public bool IsConnected => isLoggedIn && lastPacketReceived > TimeSpan.Zero;
        public LoginRefuseReason? RefuseReason { get; private set; } = null;
        public OnlineState State { get; }
        public SemaphoreSlim StateSemaphore { get; } = new SemaphoreSlim(1);

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
            else if (message is PlayerCountMessage playerCountMessage)
            {
                await HandlePlayerCount(playerCountMessage);
            }
            else if (message is StageUpdateMessage stageUpdateMessage)
            {
                await HandleStageUpdate(stageUpdateMessage, result.TimeReceived);
            }
            else if (message is PlayerDetailsMessage playerDetailsMessage)
            {
                await HandlePlayerDetails(playerDetailsMessage);
            }
            else if (message is ChatMessage chatMessage)
            {
                await HandleChat(chatMessage);
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
            if (State.IncomingChats == null)
            {
                State.OutgoingChats = new Common.Chat.OutgoingChatBuffer();
                State.IncomingChats = new Common.Chat.IncomingChatBuffer(message.LatestChatIndex);
            }
            State.OutgoingChats.SetRequestedIndex(message.RequestedChatIndex);
            StateSemaphore.Release();
        }

        private async Task HandlePlayerCount(PlayerCountMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            if (State.Location == OnlineState.OnlineLocation.Menu)
            {
                for (int i = 0; i < Definitions.CourseIds.Count; i++)
                {
                    State.CoursePlayerCounts[Definitions.CourseIds[i]] = message.CourseCounts[i];
                }
                for (int i = 0; i < Definitions.StageIds.Count; i++)
                {
                    State.StagePlayerCounts[Definitions.StageIds[i]] = message.StageCounts[i];
                }
            }
            StateSemaphore.Release();
        }

        private async Task HandleStageUpdate(StageUpdateMessage message, TimeSpan timeReceived)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            if (State.Location == OnlineState.OnlineLocation.Game)
            {
                State.AddSnapshot(message, timeReceived);
            }
            StateSemaphore.Release();
        }

        private async Task HandlePlayerDetails(PlayerDetailsMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            if (State.Players.TryGetValue(message.PlayerId, out OnlinePlayer player))
            {
                player.SetDetails(message.Name, (byte)(message.Character & 31), (byte)(message.Character >> 5), message.CustomisationsNum, message.CustomisationsChara);
            }
            StateSemaphore.Release();
        }

        private async Task HandleChat(ChatMessage message)
        {
            if (message.Secret != secret) return;

            await StateSemaphore.WaitAsync();
            State.IncomingChats?.ReceiveChatFromRemote(message.Index, message.Content);
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
                    Password = password
                };
                await SendAsync(loginMessage.Encode(), serverEndpoint);
            }
            else
            {
                if (State.Location == OnlineState.OnlineLocation.Menu)
                {
                    //Send status. For now this just acts as a keepalive to stop us getting disconnected
                    byte[] statusBytes = new MenuStatusMessage()
                    {
                        Secret = secret,
                        RequestedChatIndex = State.IncomingChats?.IndexToRequest ?? 0
                    }.Encode();
                    await SendAsync(statusBytes, serverEndpoint);
                }
                else if (State.Location == OnlineState.OnlineLocation.Game)
                {
                    //Send status
                    byte[] statusBytes = new GameStatusMessage()
                    {
                        Secret = secret,
                        RequestedChatIndex = State.IncomingChats?.IndexToRequest ?? 0,
                        Course = State.Course,
                        Stage = State.Stage,
                        Tick = CurrentTick,
                        Position = (State.MyPosition.PosX, State.MyPosition.PosY, State.MyPosition.PosZ),
                        AngularVelocity = (State.MyPosition.AngVeloX, State.MyPosition.AngVeloY, State.MyPosition.AngVeloZ),
                        MotionState = (byte)((State.MotionState & (byte)31) | (State.IsOnGround ? (byte)32 : (byte)0)),
                        Character = (byte)((State.SkinIndex << 5) | (State.Character & 31)),
                        CustomisationsNum = State.CustomisationsNum,
                        CustomisationsChara = State.CustomisationsChara,
                    }.Encode();
                    await SendAsync(statusBytes, serverEndpoint);
                    //Send player detail request if there are some players we don't have the details of
                    ushort[] playerIdsToRequest = State.Players.Values.Where(p => !p.HasDetails).Select(p => p.Id).ToArray();
                    if (playerIdsToRequest.Length > 0)
                    {
                        byte[] requestBytes = new GetPlayerDetailsMessage()
                        {
                            Secret = secret,
                            RequestedPlayers = playerIdsToRequest
                        }.Encode();
                        await SendAsync(requestBytes, serverEndpoint);
                    }
                }

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
            }

            StateSemaphore.Release();
        }
    }
}
