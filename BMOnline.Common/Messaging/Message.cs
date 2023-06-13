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
                case MessageType.MenuStatus:
                    returnMessage = new MenuStatusMessage();
                    break;
                case MessageType.GameStatus:
                    returnMessage = new GameStatusMessage();
                    break;
                case MessageType.GetPlayerDetails:
                    returnMessage = new GetPlayerDetailsMessage();
                    break;
                case MessageType.LoginRefuse:
                    returnMessage = new LoginRefuseMessage();
                    break;
                case MessageType.GlobalInfo:
                    returnMessage = new GlobalInfoMessage();
                    break;
                case MessageType.PlayerCount:
                    returnMessage = new PlayerCountMessage();
                    break;
                case MessageType.StageUpdate:
                    returnMessage = new StageUpdateMessage();
                    break;
                case MessageType.PlayerDetails:
                    returnMessage = new PlayerDetailsMessage();
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

        public byte[] Encode()
        {
            byte[] messageData = EncodeMessage();
            byte[] output;
            MessageType type;
            if (this is LoginMessage) type = MessageType.Login;
            else if (this is MenuStatusMessage) type = MessageType.MenuStatus;
            else if (this is GameStatusMessage) type = MessageType.GameStatus;
            else if (this is GetPlayerDetailsMessage) type = MessageType.GetPlayerDetails;
            else if (this is LoginRefuseMessage) type = MessageType.LoginRefuse;
            else if (this is GlobalInfoMessage) type = MessageType.GlobalInfo;
            else if (this is PlayerCountMessage) type = MessageType.PlayerCount;
            else if (this is StageUpdateMessage) type = MessageType.StageUpdate;
            else if (this is PlayerDetailsMessage) type = MessageType.PlayerDetails;
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
            MenuStatus,
            GameStatus,
            GetPlayerDetails,
            LoginRefuse,
            GlobalInfo,
            PlayerCount,
            StageUpdate,
            PlayerDetails,
            Unknown = byte.MaxValue
        }
    }
}
