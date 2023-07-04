using System.Net;
using BMOnline.Common.Chat;

namespace BMOnline.Server
{
    internal class User
    {
        private static ushort nextId = 0;

        public User(uint secret, string name, IPEndPoint endPoint, TimeSpan currentTime, ushort[] snapshotIds, ushort[] requestIds)
        {
            if (name.Length > 32)
                throw new ArgumentException("Name length exceeds 32 character limit", nameof(name));

            Id = nextId++;
            Secret = secret;
            Name = name;
            EndPoint = endPoint;
            IncomingChats = new IncomingChatBuffer(0);

            Snapshots = new Dictionary<ushort, RelaySnapshot?>();
            foreach (ushort relayId in snapshotIds)
            {
                Snapshots.Add(relayId, null);
            }
            Requests = new Dictionary<ushort, RelayRequest?>();
            RequestedPlayers = new Dictionary<ushort, List<ushort>>();
            foreach (ushort relayId in requestIds)
            {
                Requests.Add(relayId, null);
                RequestedPlayers.Add(relayId, new List<ushort>());
            }

            Stage = ushort.MaxValue;

            Renew(currentTime);
        }

        public ushort Id { get; }
        public uint Secret { get; }
        public string Name { get; }
        public IPEndPoint EndPoint { get; }
        public TimeSpan LastPacketReceived { get; private set; }

        public byte RequestedChatIndex { get; set; }
        public IncomingChatBuffer IncomingChats { get; }

        public Dictionary<ushort, RelaySnapshot?> Snapshots { get; }
        public Dictionary<ushort, RelayRequest?> Requests { get; }
        public Dictionary<ushort, List<ushort>> RequestedPlayers { get; }

        public ushort Stage { get; set; }

        public void Renew(TimeSpan currentTime)
        {
            LastPacketReceived = currentTime;
        }

        public bool IsExpired(TimeSpan currentTime) => currentTime - LastPacketReceived > TimeSpan.FromSeconds(5);
    }
}
