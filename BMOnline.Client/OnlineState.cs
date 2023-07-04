using System;
using System.Collections.Generic;
using BMOnline.Client.Relay.Requests;
using BMOnline.Client.Relay.Snapshots;
using BMOnline.Common.Chat;
using BMOnline.Common.Relay.Requests;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.Client
{
    public class OnlineState
    {
        private readonly Dictionary<ushort, RelaySnapshotType> relaySnapshotTypes;
        private readonly Dictionary<ushort, RelayRequestType> relayRequestTypes;

        public OnlineState(OnlineClient onlineClient, RelaySnapshotType[] snapshotTypes, RelayRequestType[] requestTypes)
        {
            OnlineCount = 0;
            MaxChatLength = 0;

            relaySnapshotTypes = new Dictionary<ushort, RelaySnapshotType>();
            relayRequestTypes = new Dictionary<ushort, RelayRequestType>();
            relaySnapshotTypes.Add(0, new RelaySnapshotType(0, () => new StagePositionSnapshot()));
            relayRequestTypes.Add(0, new RelayRequestType(0, () => new PlayerInfoRequest()));

            foreach (RelaySnapshotType snapshotType in snapshotTypes)
            {
                if (relaySnapshotTypes.ContainsKey(snapshotType.RelayTypeId))
                    throw new InvalidOperationException("Snapshot types contains multiple types with the same relay ID");
                relaySnapshotTypes.Add(snapshotType.RelayTypeId, snapshotType);
            }
            foreach (RelayRequestType requestType in requestTypes)
            {
                if (relaySnapshotTypes.ContainsKey(requestType.RelayTypeId))
                    throw new InvalidOperationException("Request types contains multiple types with the same relay ID");
                relayRequestTypes.Add(requestType.RelayTypeId, requestType);
            }

            GetPlayerInfoType().SendData(new PlayerInfoRequest(onlineClient.Name));
        }

        public ushort OnlineCount { get; set; }
        public ushort MaxChatLength { get; set; }

        public OutgoingChatBuffer OutgoingChats { get; set; }
        public IncomingChatBuffer IncomingChats { get; set; }

        public RelaySnapshotType GetRelaySnapshotType(ushort relayId) => relaySnapshotTypes.TryGetValue(relayId, out RelaySnapshotType snapshotType) ? snapshotType : null;
        public RelayRequestType GetRelayRequestType(ushort relayId) => relayRequestTypes.TryGetValue(relayId, out RelayRequestType requestType) ? requestType : null;

        public RelaySnapshotType GetStagePositionType() => GetRelaySnapshotType(0);
        public RelayRequestType GetPlayerInfoType() => GetRelayRequestType(0);

        public IReadOnlyCollection<ushort> GetAllPlayers() => GetPlayerInfoType().GetAllPlayers();
        public IReadOnlyCollection<RelaySnapshotType> GetAllRelaySnapshotTypes() => relaySnapshotTypes.Values;
        public IReadOnlyCollection<RelayRequestType> GetAllRelayRequestTypes() => relayRequestTypes.Values;
    }
}
