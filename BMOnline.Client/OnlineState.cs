using System;
using System.Collections.Generic;
using System.Linq;
using BMOnline.Common;
using BMOnline.Common.Chat;
using BMOnline.Common.Messaging;

namespace BMOnline.Client
{
    public class OnlineState
    {
        private readonly OnlineClient onlineClient;

        public OnlineState(OnlineClient onlineClient)
        {
            this.onlineClient = onlineClient;

            OnlineCount = 0;
            MaxChatLength = 0;
            CoursePlayerCounts = new Dictionary<byte, ushort>();
            foreach (byte courseId in Definitions.CourseIds)
            {
                CoursePlayerCounts[courseId] = 0;
            }

            StagePlayerCounts = new Dictionary<ushort, ushort>();
            foreach (ushort stageId in Definitions.StageIds)
            {
                StagePlayerCounts[stageId] = 0;
            }

            Course = byte.MaxValue;
            Stage = ushort.MaxValue;
            Players = new Dictionary<ushort, OnlinePlayer>();

            Location = OnlineLocation.Menu;
            MyPosition = new OnlinePosition();
            MotionState = 0;
            IsOnGround = false;
            CustomisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };
            CustomisationsChara = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public ushort OnlineCount { get; set; }
        public ushort MaxChatLength { get; set; }
        public Dictionary<byte, ushort> CoursePlayerCounts { get; }
        public Dictionary<ushort, ushort> StagePlayerCounts { get; }

        public OutgoingChatBuffer OutgoingChats { get; set; }
        public IncomingChatBuffer IncomingChats { get; set; }

        public byte Course { get; set; }
        public ushort Stage { get; set; }
        public Dictionary<ushort, OnlinePlayer> Players { get; }

        public enum OnlineLocation
        {
            Menu,
            Game
        }
        public OnlineLocation Location { get; set; }
        public OnlinePosition MyPosition { get; set; }
        public byte MotionState { get; set; }
        public bool IsOnGround { get; set; }
        public byte Character { get; set; }
        public byte SkinIndex { get; set; }
        public byte[] CustomisationsNum { get; set; }
        public byte[] CustomisationsChara { get; set; }

        public void AddSnapshot(StageUpdateMessage message, TimeSpan snapshotTime)
        {
            if (message.Stage != Stage) return; //If this is for the wrong stage, don't change anything. This packet is outdated

            foreach (StageUpdateMessage.StagePlayer playerInfo in message.Players)
            {
                if (!Players.TryGetValue(playerInfo.Id, out OnlinePlayer player))
                {
                    player = new OnlinePlayer(playerInfo.Id, onlineClient);
                    Players.Add(player.Id, player);
                }
                player.AddSnapshot(playerInfo.Tick,
                    new OnlinePosition(playerInfo.Positon.Item1, playerInfo.Positon.Item2, playerInfo.Positon.Item3,
                    playerInfo.AngularVelocity.Item1, playerInfo.AngularVelocity.Item2, playerInfo.AngularVelocity.Item3),
                    (byte)(playerInfo.MotionState & (byte)31),
                    (playerInfo.MotionState & (byte)32) == 32,
                    snapshotTime - TimeSpan.FromMilliseconds(playerInfo.AgeMs));
            }

            //Remove players that are gone
            OnlinePlayer[] expiredPlayers = Players.Values.Where(p => !message.Players.Select(mp => mp.Id).Contains(p.Id)).ToArray(); //Is this too inefficient?
            foreach (var player in expiredPlayers)
            {
                Players.Remove(player.Id);
            }
        }

        public void ClearPlayers() => Players.Clear();
    }
}
