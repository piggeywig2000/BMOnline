using BMOnline.Common;
using BMOnline.Common.Gamemodes;
using BMOnline.Common.Messaging;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Server.Gamemodes
{
    internal class RacePlayer
    {
        public RacePlayer(ushort id)
        {
            Id = id;
            Reset();
        }

        public ushort Id { get; }
        public bool IsLoaded { get; set; }
        public float FinishTime { get; set; }
        public bool IsFinished => FinishTime > 0;

        public void Reset()
        {
            IsLoaded = false;
            FinishTime = 0;
        }
    }

    internal class RaceGamemode : IGamemodeBase
    {
        private static readonly Random random = new Random();

        private readonly Dictionary<ushort, RacePlayer> players;
        private readonly float timeLimitMultiplier = 5;

        private TimeSpan stateTime;
        private float timeLimit;

        public RaceGamemode(UserManager userManager, bool isTimeAttack, float timeLimitMultiplier)
        {
            players = new Dictionary<ushort, RacePlayer>();
            this.timeLimitMultiplier = timeLimitMultiplier;
            IsTimeAttack = isTimeAttack;

            Reset();
            userManager.RegisterGamemode(this);
        }

        public bool IsTimeAttack { get; }
        public OnlineGamemode GamemodeType => IsTimeAttack ? OnlineGamemode.TimeAttackMode : OnlineGamemode.RaceMode;

        public RaceState State { get; private set; }
        public ushort CurrentStageId { get; private set; }

        public void AddUser(User user)
        {
            if (players.ContainsKey(user.Id))
                return;
            players.Add(user.Id, new RacePlayer(user.Id));
        }

        public void RemoveUser(ushort user)
        {
            players.Remove(user);
        }

        public bool UserInGamemode(ushort user) => players.ContainsKey(user);

        public float GetTimeRemaining(TimeSpan currentTime)
        {
            if (State == RaceState.Playing)
                return (float)Math.Max(0, timeLimit - (currentTime - stateTime).TotalSeconds);
            else if (State == RaceState.WaitingForLoad)
                return timeLimit;
            return 0;
        }

        private void Reset()
        {
            State = RaceState.Inactive;
            CurrentStageId = 0;
            stateTime = TimeSpan.Zero;
            timeLimit = 0;

            foreach (RacePlayer player in players.Values)
                player.Reset();
        }

        private void NextStage()
        {
            ushort previousStageId = CurrentStageId;
            Reset();
            State = RaceState.WaitingForLoad;

            ushort nextStageId = previousStageId;
            while (nextStageId == previousStageId)
                nextStageId = Definitions.RaceStages[random.Next(Definitions.RaceStages.Count)];
            CurrentStageId = nextStageId;

            if (!Definitions.TimeLimits.TryGetValue(CurrentStageId, out int stageTimeLimit))
                stageTimeLimit = 60;
            timeLimit = stageTimeLimit * timeLimitMultiplier;
        }

        private void StartStage(TimeSpan currentTime)
        {
            stateTime = currentTime;
            State = RaceState.Playing;
        }

        private void FinishStage(TimeSpan currentTime)
        {
            stateTime = currentTime;
            State = RaceState.Finished;
        }

        public void Update(TimeSpan currentTime)
        {
            if (players.Count == 0)
            {
                if (State != RaceState.Inactive)
                {
                    Reset();
                }
                return;
            }

            if (State == RaceState.Inactive || (State == RaceState.Finished && currentTime - stateTime >= TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("next");
                NextStage();
            }

            if (State == RaceState.WaitingForLoad && players.Values.All(p => p.IsLoaded))
            {
                Console.WriteLine("start");
                StartStage(currentTime);
            }

            if (State == RaceState.Playing && ((!IsTimeAttack && players.Values.All(p => p.IsFinished)) || GetTimeRemaining(currentTime) <= 0))
            {
                Console.WriteLine("finish");
                FinishStage(currentTime);
            }
        }

        public RelaySnapshotReceiveMessage.RelaySnapshotPlayer GetRaceStateToSend(TimeSpan currentTime, uint tick) => new RelaySnapshotReceiveMessage.RelaySnapshotPlayer()
        {
            Id = 0,
            Tick = tick,
            AgeMs = 0,
            RelayData = new RaceStateSnapshot((byte)GamemodeType, CurrentStageId, State, GetTimeRemaining(currentTime), players.Values.Select(p => new RaceStateSnapshot.RaceStatePlayer(p.Id, p.FinishTime)).ToArray()).Encode()
        };

        public void UpdateRaceState(ushort user, RaceStateUpdateMessage updateMessage)
        {
            if (!players.TryGetValue(user, out RacePlayer? player) || updateMessage.Stage != CurrentStageId)
                return;

            player.IsLoaded = updateMessage.IsLoaded;
            player.FinishTime = updateMessage.FinishTime;
        }
    }
}
