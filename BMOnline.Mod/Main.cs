using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BMOnline.Client;
using BMOnline.Common;
using BMOnline.Mod.Chat;
using BMOnline.Mod.Patches;
using Flash2;
using Framework;
using UnityEngine;

namespace BMOnline.Mod
{
    public static class Main
    {
        private static OnlineClient client;
        private static Task clientLoop;
        private static bool hasInited = false;
        private static bool hasFatalErrored = false;
        private static bool hasShownWelcomeChat = false;

        private static ModSettings settings;

        private static NotificationsManager notificationsManager;
        private static ConnectStateManager connectStateManager;
        private static PlayerCountManager playerCountManager;
        private static ChatManager chatManager;
        private static MgCourseDataManager courseDataManager = null;
        private static Transform objRoot = null;

        private static Quaternion lastRotationValue = Quaternion.identity;
        private static bool didRotateLastFrame = false;
        private static Vector3 lastAngularVelocity = Vector3.zero;

        private static byte[] customisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };
        private static byte[] customisationsChara = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static readonly Dictionary<ushort, NetPlayer> idToPlayer = new Dictionary<ushort, NetPlayer>();
        private static readonly Dictionary<ushort, NetPlayer> idToLoadingPlayer = new Dictionary<ushort, NetPlayer>();

        public static MainGameStage mainGameStage = null;
        public static bool wasMainStageCreated = false;
        public static bool wasMainStageDestroyed = false;
        public static bool wasMainStageReset = false;

        public static void OnModLoad(Dictionary<string, object> settingsDict)
        {
            Log.Info("Loading online multiplayer mod");
            settings = new ModSettings(settingsDict);
        }

        public static void OnModLateUpdate()
        {
            settings?.CheckHotkeys();
            notificationsManager?.Update();

            //Check for fatal networking errors
            if (hasFatalErrored)
            {
                connectStateManager.SetDisconnected("Could not connect to server");
                return;
            }
            if (client != null && client.RefuseReason != null)
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

            //Initiate networking
            if (!hasInited && !hasFatalErrored)
            {
                MainGameStagePatch.CreateDetour();
                PlayerMotionPatch.CreateDetour();
                AppInputPatch.CreateDetour();

                notificationsManager = new NotificationsManager(settings);
                connectStateManager = new ConnectStateManager();
                playerCountManager = new PlayerCountManager(settings);
                chatManager = new ChatManager(settings);
                courseDataManager = GameObject.Find("MgCourseDataManager").GetComponent<MgCourseDataManager>();

                string name = SteamManager.GetFriendsHandler().GetPersonaName();
                if (string.IsNullOrWhiteSpace(name))
                    name = "Player";
                else if (name.Length > 32)
                    name = name.Substring(0, 32);
                try
                {
                    client = new OnlineClient(settings.ServerIpAddress, settings.ServerPort, name, settings.ServerPassword);
                }
                catch (SocketException e)
                {
                    Log.Error($"Failed to connect to the server. The server is not running or unreachable. Exception details:\n{e}");
                    hasFatalErrored = true;
                    return;
                }
                catch (Exception e)
                {
                    Log.Error($"An unknown error occurred while connecting to the server. Exception details:\n{e}");
                    hasFatalErrored = true;
                    return;
                }

                clientLoop = Task.Run(client.RunBusy);

                hasInited = true;
            }

            //Ensure networking client is still running
            if (clientLoop.IsCompleted)
            {
                Log.Error($"An unknown networking error occurred. Exception details:\n{clientLoop.Exception}");
                hasFatalErrored = true;
                return;
            }

            bool isInGame = mainGameStage != null && !mainGameStage.m_IsFullReplay;
            OnlineState.OnlineLocation location = isInGame ? OnlineState.OnlineLocation.Game : OnlineState.OnlineLocation.Menu;

            //If stage destroyed, clear other players
            if (wasMainStageDestroyed)
            {
                foreach (NetPlayer netPlayer in idToPlayer.Values)
                {
                    if (netPlayer.GameObject != null)
                        GameObject.Destroy(netPlayer.GameObject);
                }
                idToPlayer.Clear();
                idToLoadingPlayer.Clear();
                objRoot = null;
            }
            //If stage created, initialise values and get customisations
            if (wasMainStageCreated)
            {
                objRoot = GameObject.Find("ObjRoot").transform;
                lastRotationValue = Quaternion.identity;
                didRotateLastFrame = false;
                lastAngularVelocity = Vector3.zero;

                Player player = mainGameStage.GetPlayer();
                if (player != null)
                {
                    customisationsNum = new byte[10];
                    customisationsChara = new byte[10];
                    CharaCustomize.PartsSet partsSet = CharaCustomizeManager.PartsSet.Get(player.m_CharaSelectDatum);
                    for (int i = 0; i < 10; i++)
                    {
                        CharaCustomize.PartsKey partsKey = partsSet.Get((CharaCustomize.eAssignPos)i);
                        customisationsNum[i] = (byte)(partsKey == null ? byte.MaxValue : partsKey.m_Number);
                        customisationsChara[i] = (byte)(partsKey == null ? Chara.eKind.Invalid : partsKey.m_CharaKind);
                    }
                }
            }

            if (!isInGame)
            {
                //Recreate player count objects if needed
                playerCountManager.RecreatePlayerCountsIfNeeded();
            }

            if (!client.IsConnected)
                connectStateManager.SetConnecting();

            connectStateManager.SetVisibility(!isInGame || Pause.isEnable);

            //Update chat
            if (client.IsConnected && !hasShownWelcomeChat)
            {
                chatManager.AddChatMessage("Welcome to Banana Mania Online!\nPress 'T' to type in the chat.\nPress F1 to see keybinds.");
                hasShownWelcomeChat = true;
            }
            string outgoingMessage = chatManager.UpdateAndGetSubmittedChat();

            if (client.StateSemaphore.Wait(0))
            {
                //Update players online text
                if (client.IsConnected)
                    connectStateManager.SetConnected(client.State.OnlineCount);

                //Update client state
                client.State.Location = location;
                if (isInGame)
                {
                    Player player = mainGameStage.GetPlayer();
                    client.State.Course = mainGameStage.gameKind == MainGameDef.eGameKind.TimeAttack || mainGameStage.gameKind == MainGameDef.eGameKind.Challenge ? (byte)courseDataManager.m_CurrentCourse : (byte)0;
                    client.State.Stage = (ushort)mainGameStage.stageIndex;
                    client.State.MyPosition = new OnlinePosition(
                        player.transform.localPosition.x,
                        player.transform.localPosition.y,
                        player.transform.localPosition.z,
                        lastAngularVelocity.x,
                        lastAngularVelocity.y,
                        lastAngularVelocity.z);
                    client.State.MotionState = (byte)player.GetMotion().GetState();
                    client.State.IsOnGround = player.IsOnGround();
                    client.State.Character = (byte)player.m_CharaSelectDatum.m_CharaKind;
                    client.State.SkinIndex = (byte)player.m_CharaSelectDatum.m_SkinIndex;
                    client.State.CustomisationsNum = customisationsNum;
                    client.State.CustomisationsChara = customisationsChara;

                    //Destroy non-existant players
                    ushort[] removedPlayers = idToPlayer.Keys.Concat(idToLoadingPlayer.Keys).Where(p => !client.State.Players.ContainsKey(p)).ToArray();
                    foreach (ushort playerId in removedPlayers)
                    {
                        if (idToPlayer.ContainsKey(playerId))
                            GameObject.Destroy(idToPlayer[playerId].GameObject);
                        idToPlayer.Remove(playerId);
                        idToLoadingPlayer.Remove(playerId);
                    }

                    //Create new players
                    OnlinePlayer[] newPlayers = client.State.Players.Values.Where(p => p.HasDetails && !idToPlayer.ContainsKey(p.Id) && !idToLoadingPlayer.ContainsKey(p.Id)).ToArray();
                    foreach (OnlinePlayer newPlayer in newPlayers)
                    {
                        NetPlayer netPlayer = new NetPlayer(newPlayer.Id, newPlayer.Name);
                        netPlayer.Character = (Chara.eKind)newPlayer.Character;
                        netPlayer.SkinIndex = newPlayer.SkinIndex;
                        //If character doesn't exist default to AiAi
                        if (string.IsNullOrEmpty(MgCharaManager.Instance.getCharaDatum(((byte)netPlayer.Character * 100) + netPlayer.SkinIndex)?.m_prefabPath) || !AssetBundleCache.AssetExists(MgCharaManager.Instance.getCharaDatum(((byte)netPlayer.Character * 100) + netPlayer.SkinIndex).m_prefabPath))
                        {
                            netPlayer.Character = Chara.eKind.Aiai;
                            netPlayer.SkinIndex = 0;
                        }
                        netPlayer.Customisations = GetPartsSetFromArray(newPlayer.CustomisationsNum, newPlayer.CustomisationsChara);
                        //If customisation is invalid use default
                        netPlayer.Customisations.ForEachPartsKey(new Action<CharaCustomize.PartsKey>((partsKey) =>
                        {
                            if (CharaCustomizeManager.GetPartsInfo(partsKey) == null)
                            {
                                netPlayer.Customisations = CharaCustomizeManager.GetDefaultPartsSet(netPlayer.Character);
                            }
                        }));
                        //Start loading the customisation and character AssetBundles
                        CharaCustomizeManager.PartsSet.Load(GetPartsSetFromArray(newPlayer.CustomisationsNum, newPlayer.CustomisationsChara));
                        MgCharaManager.PreloadReq(((byte)netPlayer.Character * 100) + netPlayer.SkinIndex);
                        idToLoadingPlayer.Add(netPlayer.Id, netPlayer);
                    }

                    //Instantiate players
                    List<ushort> loadedPlayers = new List<ushort>();
                    foreach (ushort playerId in idToLoadingPlayer.Keys)
                    {
                        NetPlayer netPlayer = idToLoadingPlayer[playerId];
                        OnlinePlayer onlinePlayer = client.State.Players[playerId];

                        if (!CharaCustomizeManager.isBusy && !MgCharaManager.isBusy)
                        {
                            netPlayer.Instantiate(objRoot, mainGameStage);
                            loadedPlayers.Add(playerId);
                        }
                    }
                    foreach (ushort playerId in loadedPlayers)
                    {
                        NetPlayer netPlayer = idToLoadingPlayer[playerId];
                        idToLoadingPlayer.Remove(playerId);
                        idToPlayer.Add(playerId, netPlayer);
                    }

                    //Update player state
                    foreach (ushort playerId in idToPlayer.Keys)
                    {
                        NetPlayer netPlayer = idToPlayer[playerId];
                        OnlinePlayer onlinePlayer = client.State.Players[playerId];

                        OnlinePosition position = onlinePlayer.GetPosition() ?? onlinePlayer.GetLatestPosition(); //TODO: Consider using dead reckoning instead of just freezing
                        (float velX, float velY, float velZ) = onlinePlayer.GetVelocity();

                        netPlayer.Position = new Vector3(position.PosX, position.PosY, position.PosZ);
                        netPlayer.AngularVelocity = new Vector3(position.AngVeloX, position.AngVeloY, position.AngVeloZ);
                        netPlayer.Velocity = new Vector3(velX, velY, velZ);
                        byte? motionState = onlinePlayer.GetMotionState();
                        netPlayer.MotionState = motionState.HasValue ? (PlayerMotion.State)motionState.Value : netPlayer.MotionState;
                        netPlayer.IsOnGround = onlinePlayer.GetIsOnGround() ?? netPlayer.IsOnGround;
                    }
                }

                if (!isInGame)
                {
                    //Update player counts
                    playerCountManager.UpdatePlayerCounts(client.State.CoursePlayerCounts, client.State.StagePlayerCounts, courseDataManager, client.CurrentTick);
                }

                //Send/receive chats
                if (chatManager.MaxChatLength != client.State.MaxChatLength)
                    chatManager.MaxChatLength = client.State.MaxChatLength;
                if (outgoingMessage != null && client.State.OutgoingChats != null)
                    client.State.OutgoingChats.SendChat(outgoingMessage);
                if (client.State.IncomingChats != null && client.State.IncomingChats.HasReceivedChat)
                {
                    string incomingMessage = client.State.IncomingChats.GetReceivedChat();
                    chatManager.AddChatMessage(incomingMessage);
                }

                client.StateSemaphore.Release();

                //Update player position and nametag and visibility
                foreach (ushort playerId in idToPlayer.Keys)
                {
                    NetPlayer netPlayer = idToPlayer[playerId];

                    netPlayer.PhysicalTransform.localPosition = netPlayer.Position;
                    netPlayer.PhysicalTransform.Rotate(netPlayer.AngularVelocity * Time.deltaTime, Space.World);
                    netPlayer.Rotation = netPlayer.PhysicalTransform.rotation;
                    netPlayer.GravityTilt.update();
                    netPlayer.PhysicalTransform.localPosition = netPlayer.BehaviourTransform.localPosition;

                    if (netPlayer.NameTagEnabled != settings.ShowNameTags)
                        netPlayer.NameTagEnabled = settings.ShowNameTags;

                    if (settings.ShowNameTags)
                    {
                        netPlayer.NameTag.transform.localPosition = netPlayer.BehaviourTransform.localPosition + (MainGame.isViewStage ? new Vector3(0, 3f, 0) : new Vector3(0, 1f, 0));
                        Vector3 lookAtPos = MainGame.isViewStage ? MainGame.Instance.m_viewStageCamera.transform.position : (MainGame.isViewPlayer ? MainGame.Instance.m_photoModeCamera.transform.position : mainGameStage.m_CameraController.GetMainCamera().transform.position);
                        lookAtPos = new Vector3(lookAtPos.x, netPlayer.NameTag.transform.position.y, lookAtPos.z);
                        netPlayer.NameTag.transform.LookAt(lookAtPos);
                        netPlayer.NameTag.transform.localScale = MainGame.isViewStage ? new Vector3(4, 4, 4) : Vector3.one;
                        if (netPlayer.NameTagText.fontSize != settings.NameTagSize)
                            netPlayer.NameTagText.fontSize = settings.NameTagSize;
                    }

                    bool isVisible = true;
                    switch (settings.PlayerVisibility)
                    {
                        case ModSettings.PlayerVisibilityOption.ShowAll:
                            isVisible = true;
                            break;
                        case ModSettings.PlayerVisibilityOption.HideNear:
                            if (!MainGame.isViewStage && !MainGame.isViewPlayer)
                            {
                                //Set visibility based on distance 
                                Vector3 localPlayerPos = mainGameStage.GetPlayer().transform.position;
                                Vector3 cameraPos = mainGameStage.m_CameraController.GetMainCamera().transform.position;
                                Vector3 remotePlayerPos = netPlayer.BehaviourTransform.position;
                                float lineLengthSquared = Mathf.Pow(localPlayerPos.x - cameraPos.x, 2) + Mathf.Pow(localPlayerPos.y - cameraPos.y, 2) + Mathf.Pow(localPlayerPos.z - cameraPos.z, 2);
                                float remoteDistance;
                                if (lineLengthSquared == 0)
                                    remoteDistance = Mathf.Pow(remotePlayerPos.x - localPlayerPos.x, 2) + Mathf.Pow(remotePlayerPos.y - localPlayerPos.y, 2) + Mathf.Pow(remotePlayerPos.z - localPlayerPos.z, 2);
                                else
                                {
                                    float t = ((remotePlayerPos.x - localPlayerPos.x) * (cameraPos.x - localPlayerPos.x) + (remotePlayerPos.y - localPlayerPos.y) * (cameraPos.y - localPlayerPos.y) + (remotePlayerPos.z - localPlayerPos.z) * (cameraPos.z - localPlayerPos.z)) / lineLengthSquared;
                                    t = Math.Max(Math.Min(t, 1), 0);
                                    remoteDistance =
                                        Mathf.Pow(remotePlayerPos.x - (localPlayerPos.x + (t * (cameraPos.x - localPlayerPos.x))), 2) +
                                        Mathf.Pow(remotePlayerPos.y - (localPlayerPos.y + (t * (cameraPos.y - localPlayerPos.y))), 2) +
                                        Mathf.Pow(remotePlayerPos.z - (localPlayerPos.z + (t * (cameraPos.z - localPlayerPos.z))), 2);
                                }
                                isVisible = remoteDistance >= Math.Pow(settings.PersonalSpace, 2);
                            }
                            else
                                isVisible = true;
                            break;
                        case ModSettings.PlayerVisibilityOption.HideAll:
                            isVisible = false;
                            break;
                    }
                    if (netPlayer.GameObject.activeSelf != isVisible)
                        netPlayer.GameObject.SetActive(isVisible);
                }

                //Reset other players if stage was reset
                if (isInGame && wasMainStageReset)
                {
                    foreach (NetPlayer netPlayer in idToPlayer.Values)
                    {
                        netPlayer.Reset(mainGameStage);
                    }
                    wasMainStageReset = false;
                }
            }

            wasMainStageCreated = false;
            wasMainStageDestroyed = false;
        }

        public static void OnModFixedUpdate()
        {
            if (!hasInited) return;

            //Calculate angular velocity
            if (mainGameStage != null && mainGameStage.GetPlayer() != null && Time.fixedDeltaTime != 0)
            {
                Player player = mainGameStage.GetPlayer();
                if (didRotateLastFrame)
                {
                    Quaternion delta = player.transform.rotation * Quaternion.Inverse(lastRotationValue);
                    delta.ToAngleAxis(out float angle, out Vector3 axis);
                    lastAngularVelocity = (axis * angle) / (Time.fixedDeltaTime);
                }
                lastRotationValue = player.transform.rotation;
                didRotateLastFrame = true;
            }
            else
            {
                didRotateLastFrame = false;
            }

            foreach (ushort playerId in idToPlayer.Keys)
            {
                NetPlayer netPlayer = idToPlayer[playerId];

                //Setting these values on PhysicsBall allows Player.velocity to work properly
                //Which is needed for the Monkey to work properly
                Player player = netPlayer.PlayerObject;
                PhysicsBall physicsBall = player.m_PhysicsBall;
                float deltaTime = Time.fixedDeltaTime * MgTimeScaleManager.TimeScale;
                physicsBall.m_OldPos = netPlayer.Position + ((-netPlayer.Velocity) * deltaTime);
                physicsBall.m_Pos = netPlayer.Position;
            }
        }

        public static void OnModUpdate()
        {
            if (!hasInited) return;

            foreach (ushort playerId in idToPlayer.Keys)
            {
                NetPlayer netPlayer = idToPlayer[playerId];

                //Set isOnGround
                netPlayer.PlayerObject.m_IsOnGround = netPlayer.IsOnGround;

                //Update monkey animations
                PlayerMotionPatch.PreventSetState = true;
                try
                {
                    if (netPlayer.Motion.GetState() != netPlayer.MotionState)
                    {
                        netPlayer.Motion.SetState((PlayerMotion.State)((int)netPlayer.MotionState | (1 << 20)));
                    }

                    netPlayer.Motion.ResetFloatingSec();
                    netPlayer.Motion.OnUpdate();
                }
                finally
                {
                    PlayerMotionPatch.PreventSetState = false;
                }
            }
        }

        private static CharaCustomize.PartsSet GetPartsSetFromArray(byte[] customisationsNum, byte[] customisationsChara)
        {
            CharaCustomize.PartsSet returnSet = new CharaCustomize.PartsSet();
            for (int i = 0; i < 10; i++)
            {
                if (customisationsNum[i] != byte.MaxValue)
                {
                    returnSet.Set(new CharaCustomize.PartsKey() { m_CharaKind = (Chara.eKind)customisationsChara[i], m_AssignPos = (CharaCustomize.eAssignPos)i, m_Number = customisationsNum[i] });
                }
            }
            return returnSet;
        }
    }
}
