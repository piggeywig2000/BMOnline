using System.Collections.Generic;

namespace BMOnline.Common.Chat
{
    public class IncomingChatBuffer
    {
        private readonly Dictionary<byte, string> buffer = new Dictionary<byte, string>();
        private byte localIndex = 0;
        private byte remoteIndex = 0;

        public IncomingChatBuffer(byte initialIndexValue)
        {
            localIndex = initialIndexValue;
            remoteIndex = initialIndexValue;
        }

        public byte IndexToRequest { get => remoteIndex; }

        public bool HasReceivedChat => buffer.ContainsKey(localIndex) && buffer[localIndex] != null;

        public void ReceiveChatFromRemote(byte index, string message)
        {
            if (remoteIndex != index) return;
            buffer[remoteIndex] = message;
            remoteIndex++;
            buffer[remoteIndex] = null;
        }

        public string GetReceivedChat()
        {
            if (buffer.TryGetValue(localIndex, out string message) && message != null)
            {
                localIndex++;
            }
            return message;
        }
    }
}
