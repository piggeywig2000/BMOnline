using System;
using BMOnline.Common.Gamemodes;

namespace BMOnline.Common.Relay.Snapshots
{
    public class RaceStateSnapshot : ISnapshotPacket
    {
        public struct RaceStatePlayer
        {
            public RaceStatePlayer(ushort id, float finishTime)
            {
                Id = id;
                FinishTime = finishTime;
            }

            public ushort Id { get; private set; }
            public float FinishTime { get; private set; }
        }

        public RaceStateSnapshot() : this(0, 0, RaceState.Inactive, 0, Array.Empty<RaceStatePlayer>()) { }
        public RaceStateSnapshot(byte gameKind, ushort stage, RaceState state, float timeRemaining, RaceStatePlayer[] players)
        {
            GameKind = gameKind;
            Stage = stage;
            State = state;
            TimeRemaining = timeRemaining;
            Players = players;
        }

        public byte GameKind { get; private set; }
        public ushort Stage { get; private set; }
        public RaceState State { get; private set; }
        public float TimeRemaining { get; private set; }
        public RaceStatePlayer[] Players { get; private set; }

        public void Decode(byte[] data)
        {
            GameKind = data[0];
            Stage = BitConverter.ToUInt16(data, 1);
            State = (RaceState)data[3];
            TimeRemaining = BitConverter.ToSingle(data, 4);
            ushort playerLength = BitConverter.ToUInt16(data, 8);
            Players = new RaceStatePlayer[playerLength];
            for (int i = 0; i < playerLength; i++)
            {
                Players[i] = new RaceStatePlayer(BitConverter.ToUInt16(data, 10 + (i * 6)), BitConverter.ToSingle(data, 10 + (i * 6) + 2));
            }
        }

        public byte[] Encode()
        {
            byte[] output = new byte[10 + (Players.Length * 6)];
            output[0] = GameKind;
            BitConverter.GetBytes(Stage).CopyTo(output, 1);
            output[3] = (byte)State;
            BitConverter.GetBytes(TimeRemaining).CopyTo(output, 4);
            BitConverter.GetBytes((ushort)Players.Length).CopyTo(output, 8);
            for (int i = 0; i < Players.Length; i++)
            {
                BitConverter.GetBytes(Players[i].Id).CopyTo(output, 10 + (i * 6));
                BitConverter.GetBytes(Players[i].FinishTime).CopyTo(output, 10 + (i * 6) + 2);
            }
            return output;
        }

        public ISnapshotPacket LerpTo(ISnapshotPacket destination, float time, float gapSeconds)
        {
            RaceStateSnapshot to = (RaceStateSnapshot)destination;
            return new RaceStateSnapshot(GameKind, Stage, State, TimeRemaining + (to.TimeRemaining - TimeRemaining) * time, Players);
        }
    }
}
