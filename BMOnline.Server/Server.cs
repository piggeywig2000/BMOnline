using System.Net;
using BMOnline.Common;
using BMOnline.Common.Chat;
using BMOnline.Common.Messaging;
using BMOnline.Common.Relay.Requests;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Server
{
    internal class Server : BidirectionalUdp
    {
        private readonly UserManager userManager;
        private readonly string? password;
        private readonly ushort maxChatLength;
        private readonly List<(IPEndPoint, uint, LoginRefuseReason)> loginRefusals;
        private readonly OutgoingChatBuffer outgoingChats;

        public Server(IPEndPoint localEP, string? password, ushort maxChatLength) : base(localEP)
        {
            userManager = new UserManager();
            this.password = password;
            this.maxChatLength = maxChatLength;
            loginRefusals = new List<(IPEndPoint, uint, LoginRefuseReason)>();
            outgoingChats = new OutgoingChatBuffer();
        }

        protected override Task HandleReceive(TimedUdpReceive result)
        {
            Message message = Message.Decode(result.Buffer);
            if (message is LoginMessage loginMessage)
            {
                HandleLogin(loginMessage, result.RemoteEndPoint);
            }
            else if (message is StatusMessage statusMessage)
            {
                HandleStatus(statusMessage);
            }
            else if (message is ChatMessage chatMessage)
            {
                HandleChat(chatMessage);
            }
            else if (message is RelaySnapshotSendMessage snapshotSendMessage)
            {
                HandleSnapshotSendMessage(snapshotSendMessage, result.TimeReceived);
            }
            else if (message is RelayRequestGetMessage requestGetMessage)
            {
                HandleRequestGetMessage(requestGetMessage);
            }
            else if (message is RelayRequestUpdateMessage requestUpdateMessage)
            {
                HandleRequestUpdateMessage(requestUpdateMessage);
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
            message.Name = message.Name.RemoveWhitespace().Trim().RemoveRichText().RemoveDoubleSpaces();

            //Name cannot be longer than 32 characters and must be longer than 0 characters
            if (message.Name.Length > 32 || message.Name.Length == 0)
            {
                loginRefusals.Add((remoteEndPoint, message.Secret, LoginRefuseReason.BadName));
                return;
            }

            User newUser = new User(message.Secret, message.Name, remoteEndPoint, Time, message.SnapshotIds, message.RequestIds);
            userManager.AddUser(newUser);
            outgoingChats.SendChat($"<color=yellow>{newUser.Name} joined the server</color>");
            Log.Info($"{newUser.Name} (ID {newUser.Id}) logged in");
        }

        private void HandleStatus(StatusMessage message)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet

            user.Renew(Time);
            user.RequestedChatIndex = message.RequestedChatIndex;
        }

        private void HandleChat(ChatMessage message)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet
            user.Renew(Time);
            string content = message.Content.RemoveWhitespace().Trim().RemoveRichText().RemoveDoubleSpaces();
            if (content.Length > maxChatLength)
                content = content.Substring(0, maxChatLength);
            user.IncomingChats.ReceiveChatFromRemote(message.Index, content);
            if (user.IncomingChats.HasReceivedChat)
            {
                content = user.IncomingChats.GetReceivedChat();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    outgoingChats.SendChat($"[{user.Name}] {content}");
                    Log.Info($"Chat: [{user.Name}] {content}");
                }
            }
        }

        private void HandleSnapshotSendMessage(RelaySnapshotSendMessage message, TimeSpan receivedOn)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet

            user.Renew(Time);

            if (user.Snapshots.TryGetValue(message.RelayId, out RelaySnapshot? existingSnapshot) && existingSnapshot?.Tick != message.Tick)
            {
                user.Snapshots[message.RelayId] = new RelaySnapshot(message.RelayId, message.Tick, receivedOn, message.BroadcastType, message.BroadcastTypeOperand, message.RelayData);
            }
        }

        private void HandleRequestGetMessage(RelayRequestGetMessage message)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet

            user.Renew(Time);

            if (user.RequestedPlayers.TryGetValue(message.RelayId, out List<ushort>? requests))
            {
                requests.RemoveAll(playerId => !message.RequestedPlayers.Contains(playerId));
                foreach (ushort playerId in message.RequestedPlayers.Where(playerId => userManager.UserIdExists(playerId) && !requests.Contains(playerId)))
                {
                    requests.Add(playerId);
                }
            }
        }

        private void HandleRequestUpdateMessage(RelayRequestUpdateMessage message)
        {
            if (!userManager.TryGetUserFromSecret(message.Secret, out User? user))
                return; //If user not found, drop packet

            user.Renew(Time);

            if (user.Requests.TryGetValue(message.RelayId, out RelayRequest? existingRequest) && existingRequest?.RequestId != message.RequestId)
            {
                //If player info request, update player stage
                if (message.RelayId == 0)
                {
                    PlayerInfoRequest playerInfoRequest = new PlayerInfoRequest();
                    playerInfoRequest.Decode(message.RelayData);
                    userManager.ChangeUserStage(user.Secret, playerInfoRequest.Stage);
                    playerInfoRequest = new PlayerInfoRequest(user.Name, playerInfoRequest.Mode, playerInfoRequest.Course, playerInfoRequest.Stage, playerInfoRequest.Character, playerInfoRequest.SkinIndex, playerInfoRequest.CustomisationsNum, playerInfoRequest.CustomisationsChara);
                    message.RelayData = playerInfoRequest.Encode();
                }
                user.Requests[message.RelayId] = new RelayRequest(message.RelayId, message.RequestId, message.RelayData);
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

            //Send status updates to each user
            foreach (User user in userManager.Users)
            {
                //Send global info message
                byte[] gimBytes = new GlobalInfoMessage()
                {
                    Secret = user.Secret,
                    OnlineCount = (ushort)userManager.TotalCount,
                    LatestChatIndex = outgoingChats.LatestIndex,
                    RequestedChatIndex = user.IncomingChats.IndexToRequest,
                    MaxChatLength = maxChatLength
                }.Encode();
                await SendAsync(gimBytes, user.EndPoint);
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
                //Send snapshots
                Dictionary<ushort, List<User>> snapshotIdToRelevantUsers = new Dictionary<ushort, List<User>>();
                foreach (User currentUser in userManager.Users)
                {
                    foreach (KeyValuePair<ushort, RelaySnapshot?> kvp in currentUser.Snapshots.Where(kvp => kvp.Value != null && user.Snapshots.ContainsKey(kvp.Key)))
                    {
                        if (kvp.Value!.BroadcastType == RelaySnapshotBroadcastType.EveryoneOnServer ||
                            (kvp.Value!.BroadcastType == RelaySnapshotBroadcastType.EveryoneOnStage && kvp.Value!.BroadcastTypeOperand == user.Stage) ||
                            (kvp.Value!.BroadcastType == RelaySnapshotBroadcastType.SpecificPlayer && kvp.Value!.BroadcastTypeOperand == user.Id))
                        {
                            if (!snapshotIdToRelevantUsers.ContainsKey(kvp.Key))
                                snapshotIdToRelevantUsers[kvp.Key] = new List<User>();
                            snapshotIdToRelevantUsers[kvp.Key].Add(currentUser);
                        }
                    }
                }
                foreach (KeyValuePair<ushort, List<User>> kvp in snapshotIdToRelevantUsers)
                {
                    byte[] snapshotReceiveBytes = new RelaySnapshotReceiveMessage()
                    {
                        Secret = user.Secret,
                        RelayId = kvp.Key,
                        Players = kvp.Value.Select(player =>
                        {
                            RelaySnapshot snapshot = player.Snapshots[kvp.Key]!;
                            return new RelaySnapshotReceiveMessage.RelaySnapshotPlayer()
                            {
                                Id = player.Id,
                                Tick = snapshot.Tick,
                                AgeMs = (ushort)Math.Min((Time - snapshot.ReceivedOn).Milliseconds, ushort.MaxValue),
                                RelayData = snapshot.RelayData
                            };
                        }).ToArray()
                    }.Encode();
                    await SendAsync(snapshotReceiveBytes, user.EndPoint);
                }
                //Send request responses
                foreach (KeyValuePair<ushort, List<ushort>> kvp in user.RequestedPlayers)
                {
                    if (kvp.Value.Count > 0)
                    {
                        ushort playerId = kvp.Value[0];
                        kvp.Value.Remove(0);
                        if (userManager.TryGetUserFromId(playerId, out User? requestedUser) && requestedUser.Requests.TryGetValue(kvp.Key, out RelayRequest? requestedRequest) && requestedRequest != null)
                        {
                            byte[] requestResponseBytes = new RelayRequestResponseMessage()
                            {
                                Secret = user.Secret,
                                RelayId = kvp.Key,
                                PlayerId = playerId,
                                RequestId = requestedRequest.RequestId,
                                RelayData = requestedRequest.RelayData
                            }.Encode();
                            await SendAsync(requestResponseBytes, user.EndPoint);
                        }
                    }
                }
                //Send request player update
                List<RelayRequestPlayersMessage.RelayRequestPlayer> requestPlayers = new List<RelayRequestPlayersMessage.RelayRequestPlayer>();
                foreach (User currentUser in userManager.Users)
                {
                    //Only include relay IDs that are not null and the user to send to also has it
                    ushort[] relayIds = currentUser.Requests.Where(kvp => kvp.Value != null && user.Requests.ContainsKey(kvp.Key)).Select(kvp => kvp.Key).ToArray();
                    byte[] requestIds = relayIds.Select(relayId => currentUser.Requests[relayId]!.RequestId).ToArray();
                    requestPlayers.Add(new RelayRequestPlayersMessage.RelayRequestPlayer()
                    {
                        Id = currentUser.Id,
                        RelayIds = relayIds,
                        RequestIds = requestIds
                    });
                }
                byte[] requestPlayerBytes = new RelayRequestPlayersMessage()
                {
                    Secret = user.Secret,
                    ClientPlayerId = user.Id,
                    Players = requestPlayers.ToArray()
                }.Encode();
                await SendAsync(requestPlayerBytes, user.EndPoint);
            }
        }
    }
}
