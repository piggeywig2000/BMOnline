using System;

namespace BMOnline.Common.Messaging
{
    public abstract class Message
    {
        public static Message Decode(byte[] data)
        {
            MessageType type = (MessageType)data[0];
            Message returnMessage;
            switch (type)
            {
                case MessageType.Login:
                    returnMessage = new LoginMessage();
                    break;
                case MessageType.Status:
                    returnMessage = new StatusMessage();
                    break;
                case MessageType.RelaySnapshotSend:
                    returnMessage = new RelaySnapshotSendMessage();
                    break;
                case MessageType.RelayRequestGet:
                    returnMessage = new RelayRequestGetMessage();
                    break;
                case MessageType.RelayRequestUpdate:
                    returnMessage = new RelayRequestUpdateMessage();
                    break;
                case MessageType.LoginRefuse:
                    returnMessage = new LoginRefuseMessage();
                    break;
                case MessageType.GlobalInfo:
                    returnMessage = new GlobalInfoMessage();
                    break;
                case MessageType.RelaySnapshotReceive:
                    returnMessage = new RelaySnapshotReceiveMessage();
                    break;
                case MessageType.RelayRequestPlayers:
                    returnMessage = new RelayRequestPlayersMessage();
                    break;
                case MessageType.RelayRequestResponse:
                    returnMessage = new RelayRequestResponseMessage();
                    break;
                case MessageType.RaceStateUpdate:
                    returnMessage = new RaceStateUpdateMessage();
                    break;
                case MessageType.Chat:
                    returnMessage = new ChatMessage();
                    break;
                default:
                    throw new InvalidOperationException("Message type to decode is not supported");
            };
            byte[] messageData = new byte[data.Length - 1];
            Array.Copy(data, 1, messageData, 0, messageData.Length);
            returnMessage.DecodeMessage(messageData);
            return returnMessage;
        }
        public static Message DecodeRaw(byte[] data, Message messageToDecodeInto)
        {
            messageToDecodeInto.DecodeMessage(data);
            return messageToDecodeInto;
        }

        protected abstract void DecodeMessage(byte[] data);
        protected abstract byte[] EncodeMessage();

        public byte[] Encode() => Encode(BidirectionalUdp.PROTOCOL_VERSION);
        public byte[] Encode(byte protocolVersion)
        {
            byte[] messageData = EncodeMessage();
            byte[] output;
            MessageType type;
            if (this is LoginMessage) type = MessageType.Login;
            else if (this is StatusMessage) type = MessageType.Status;
            else if (this is RelaySnapshotSendMessage) type = MessageType.RelaySnapshotSend;
            else if (this is RelayRequestGetMessage) type = MessageType.RelayRequestGet;
            else if (this is RelayRequestUpdateMessage) type = MessageType.RelayRequestUpdate;
            else if (this is LoginRefuseMessage) type = protocolVersion >= 4 ? MessageType.LoginRefuse : (MessageType)4;
            else if (this is GlobalInfoMessage) type = MessageType.GlobalInfo;
            else if (this is RelaySnapshotReceiveMessage) type = MessageType.RelaySnapshotReceive;
            else if (this is RelayRequestPlayersMessage) type = MessageType.RelayRequestPlayers;
            else if (this is RelayRequestResponseMessage) type = MessageType.RelayRequestResponse;
            else if (this is RaceStateUpdateMessage) type = MessageType.RaceStateUpdate;
            else if (this is ChatMessage) type = MessageType.Chat;
            else throw new InvalidOperationException("Message type to encode is not supported");

            output = new byte[messageData.Length + 1];
            output[0] = (byte)type;
            messageData.CopyTo(output, 1);
            return output;
        }

        public byte[] EncodeRaw()
        {
            return EncodeMessage();
        }

        public enum MessageType : byte
        {
            Login,
            Status,
            RelaySnapshotSend,
            RelayRequestGet,
            RelayRequestUpdate,
            LoginRefuse,
            GlobalInfo,
            RelaySnapshotReceive,
            RelayRequestPlayers,
            RelayRequestResponse,
            RaceStateUpdate,
            Chat,
            Unknown = byte.MaxValue
        }
    }
}
