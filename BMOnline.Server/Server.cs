using System.Net;
using BMOnline.Common;
using BMOnline.Common.Chat;
using BMOnline.Common.Messaging;

namespace BMOnline.Server
{
    internal class Server : BidirectionalUdp
    {
        private readonly UserManager userManager;
        private readonly string? password;
        private readonly List<(IPEndPoint, uint, LoginRefuseReason)> loginRefusals;
        private readonly OutgoingChatBuffer outgoingChats;

        public Server(IPEndPoint localEP, string? password) : base(localEP)
        {
            userManager = new UserManager();
            this.password = password;
            loginRefusals = new List<(IPEndPoint, uint, LoginRefuseReason)>();
            outgoingChats = new OutgoingChatBuffer();
        }
        public Server(IPEndPoint localEP) : this(localEP, null) { }

        protected override Task HandleReceive(TimedUdpReceive result)
        {
            Message message = Message.Decode(result.Buffer);
            if (message is LoginMessage loginMessage)
            {
                HandleLogin(loginMessage, result.RemoteEndPoint);
            }
            else if (message is StatusMessage statusMessage)
            {
                HandleStatus(statusMessage, result.TimeReceived);
            }
            else if (message is GetPlayerDetailsMessage getPlayerDetailsMessage)
            {
                HandleGetPlayerDetails(getPlayerDetailsMessage);
            }
            else if (message is ChatMessage chatMessage)
            {
                HandleChat(chatMessage);
            }
            return Task.CompletedTask;
        }

        private void HandleLogin(LoginMessage message, IPEndPoint remoteEndPoint)
        {
            if (userManager.UserSecretExists(message.Secret))
                return; //Login packets may be duplicated

            //Incorrect protocol version, probably on an old version
            if (message.ProtocolVersion != PROTOCOL_VERSION)
            {
                loginRefusals.Add((remoteEndPoint, message.Secret, LoginRefuseReason.ProtocolVersion));
                return;
            }

            //Check password
            if (!string.IsNullOrEmpty(password) && message.Password != password)
            {
                loginRefusals.Add((remoteEndPoint, message.Secret, LoginRefuseReason.Password));
                return;
            }

            //Clean name
            message.Name = message.Name.RemoveWhitespace().RemoveRichText().RemoveDoubleSpaces();

            //Name cannot be longer than 32 characters and must be longer than 0 characters
            if (message.Name.Length > 32 || message.Name.Length == 0)
            {
                loginRefusals.Add((remoteEndPoint, message.Secret, LoginRefuseReason.BadName));
                return;
            }

            User newUser = new User(message.Secret, message.Name, remoteEndPoint, Time);
            userManager.AddUser(newUser);
            outgoingChats.SendChat($"<color=yellow>{newUser.Name} joined the server</color>");
            Log.Info($"{newUser.Name} (ID {newUser.Id}) logged in");
        }

        private void HandleStatus(StatusMessage message, TimeSpan receivedOn)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet

            user.Renew(Time);
            user.RequestedChatIndex = message.RequestedChatIndex;
            if (message is MenuStatusMessage)
            {
                userManager.MoveUserToMenu(user);
            }
            else if (message is GameStatusMessage gameStatus)
            {
                userManager.MoveUserToGame(user, gameStatus.Course, gameStatus.Stage);
                user.LastPositionUpdate = receivedOn;
                user.LastPositionTick = gameStatus.Tick;
                user.Position = gameStatus.Position;
                user.AngularVelocity = gameStatus.AngularVelocity;
                user.MotionState = gameStatus.MotionState;
                user.Character = gameStatus.Character;
                user.CustomisationsNum = gameStatus.CustomisationsNum;
                user.CustomisationsChara = gameStatus.CustomisationsChara;
            }
        }

        private void HandleGetPlayerDetails(GetPlayerDetailsMessage message)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet
            user.Renew(Time);
            user.RequestedPlayerIds.RemoveAll(id => !message.RequestedPlayers.Contains(id));
            foreach (ushort playerId in message.RequestedPlayers)
            {
                if (!userManager.UserIdExists(playerId) || user.RequestedPlayerIds.Contains(playerId)) continue;
                user.RequestedPlayerIds.Add(playerId);
            }
        }

        private void HandleChat(ChatMessage message)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet
            user.Renew(Time);
            user.IncomingChats.ReceiveChatFromRemote(message.Index, message.Content.RemoveWhitespace().RemoveRichText().RemoveDoubleSpaces());
            if (user.IncomingChats.HasReceivedChat)
            {
                string content = user.IncomingChats.GetReceivedChat();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    outgoingChats.SendChat($"[{user.Name}] {content}");
                    Log.Info($"Chat: [{user.Name}] {content}");
                }
            }
        }

        protected override async Task SendTick()
        {
            User[] expiredUsers = userManager.RemoveExpired(Time);
            foreach (User expiredUser in  expiredUsers)
            {
                outgoingChats.SendChat($"<color=yellow>{expiredUser.Name} left the server</color>");
                Log.Info($"{expiredUser.Name} (ID {expiredUser.Id}) disconnected");
            }

            foreach ((IPEndPoint endPoint, uint secret, LoginRefuseReason reason) in loginRefusals)
            {
                byte[] lrmBytes = new LoginRefuseMessage()
                {
                    Secret = secret,
                    Reason = reason
                }.Encode();
                await SendAsync(lrmBytes, endPoint);
            }
            loginRefusals.Clear();

            ushort[] courseCounts = Definitions.CourseIds.Select(courseId => (ushort)userManager.GetCoursePlayerCount(courseId)).ToArray();
            byte[] stageCounts = Definitions.StageIds.Select(stageId => (byte)userManager.GetStagePlayerCount(stageId)).ToArray();

            //Send status updates to each user
            foreach (User user in userManager.Users)
            {
                byte[] gimBytes = new GlobalInfoMessage()
                {
                    Secret = user.Secret,
                    OnlineCount = (ushort)userManager.TotalCount,
                    LatestChatIndex = outgoingChats.LatestIndex,
                    RequestedChatIndex = user.IncomingChats.IndexToRequest
                }.Encode();
                await SendAsync(gimBytes, user.EndPoint);

                if (user.Location == UserLocation.Menu)
                {
                    byte[] pcmBytes = new PlayerCountMessage()
                    {
                        Secret = user.Secret,
                        CourseCounts = courseCounts,
                        StageCounts = stageCounts
                    }.Encode();
                    await SendAsync(pcmBytes, user.EndPoint);
                }
                else if (user.Location == UserLocation.Game)
                {
                    byte[] sumBytes = new StageUpdateMessage()
                    {
                        Secret = user.Secret,
                        Stage = user.Stage,
                        Players = userManager.GetUsersInStage(user.Stage)
                            .Where(u => u.Secret != user.Secret)
                            .Select(user =>
                            {
                                return new StageUpdateMessage.StagePlayer()
                                {
                                    Id = user.Id,
                                    Tick = user.LastPositionTick,
                                    AgeMs = (ushort)Math.Min((Time - user.LastPositionUpdate).Milliseconds, ushort.MaxValue),
                                    Positon = user.Position,
                                    AngularVelocity = user.AngularVelocity,
                                    MotionState = user.MotionState
                                };
                            }).ToArray()
                    }.Encode();
                    await SendAsync(sumBytes, user.EndPoint);
                    //Send player details if they requested it
                    if (user.RequestedPlayerIds.Count > 0)
                    {
                        ushort playerId = user.RequestedPlayerIds[0];
                        user.RequestedPlayerIds.RemoveAt(0);
                        if (userManager.TryGetUserFromId(playerId, out User? requestedPlayer))
                        {
                            byte[] detailBytes = new PlayerDetailsMessage()
                            {
                                Secret = user.Secret,
                                PlayerId = requestedPlayer.Id,
                                Name = requestedPlayer.Name,
                                Character = requestedPlayer.Character,
                                CustomisationsNum = requestedPlayer.CustomisationsNum,
                                CustomisationsChara = requestedPlayer.CustomisationsChara
                            }.Encode();
                            await SendAsync(detailBytes, user.EndPoint);
                        }
                    }
                }

                //Send chat
                string outgoingChat = outgoingChats.GetChatAtIndex(user.RequestedChatIndex);
                if (outgoingChat != null)
                {
                    byte[] chatBytes = new ChatMessage()
                    {
                        Secret = user.Secret,
                        Index = user.RequestedChatIndex,
                        Content = outgoingChat
                    }.Encode();
                    await SendAsync(chatBytes, user.EndPoint);
                }
            }
        }
    }
}
