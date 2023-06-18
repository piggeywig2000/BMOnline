using System.Net;
using BMOnline.Common.Chat;

namespace BMOnline.Server
{
    internal class User
    {
        private static ushort nextId = 0;

        public User(uint secret, string name, IPEndPoint endPoint, TimeSpan currentTime)
        {
            if (name.Length > 32)
                throw new ArgumentException("Name length exceeds 32 character limit", nameof(name));

            Id = nextId++;
            Secret = secret;
            Name = name;
            EndPoint = endPoint;
            RequestedPlayerIds = new List<ushort>();
            IncomingChats = new IncomingChatBuffer(0);

            Location = UserLocation.Menu;
            Course = byte.MaxValue;
            Stage = ushort.MaxValue;
            MotionState = 0;
            Character = 0;
            CustomisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };
            CustomisationsChara = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            Renew(currentTime);
        }

        public ushort Id { get; }
        public uint Secret { get; }
        public string Name { get; }
        public IPEndPoint EndPoint { get; }
        public TimeSpan LastPacketReceived { get; private set; }
        public List<ushort> RequestedPlayerIds { get; }

        public byte RequestedChatIndex { get; set; }
        public IncomingChatBuffer IncomingChats { get; }

        public UserLocation Location { get; set; }
        public byte Course { get; set; }
        public ushort Stage { get; set; }
        public TimeSpan LastPositionUpdate { get; set; }
        public uint LastPositionTick { get; set; }
        public (float, float, float) Position { get; set; }
        public (float, float, float) AngularVelocity { get; set; }
        public byte MotionState { get; set; }
        public byte Character { get; set; }
        public byte[] CustomisationsNum { get; set; }
        public byte[] CustomisationsChara { get; set; }

        public void Renew(TimeSpan currentTime)
        {
            LastPacketReceived = currentTime;
        }

        public bool IsExpired(TimeSpan currentTime) => currentTime - LastPacketReceived > TimeSpan.FromSeconds(5);
    }

    public enum UserLocation
    {
        Menu,
        Game
    }
}
