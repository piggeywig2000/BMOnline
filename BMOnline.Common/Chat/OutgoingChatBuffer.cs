using System.Collections.Generic;

namespace BMOnline.Common.Chat
{
    public class OutgoingChatBuffer
    {
        private readonly Dictionary<byte, string> buffer = new Dictionary<byte, string>();
        private byte localIndex = 0;
        private byte remoteIndex = 0;

        public OutgoingChatBuffer() { }

        public byte LatestIndex { get => localIndex; }
        public byte RequestedIndex { get => remoteIndex; }

        public void SendChat(string message)
        {
            buffer[localIndex] = message;
            localIndex++;
            buffer[localIndex] = null;
        }

        public void SetRequestedIndex(byte index)
        {
            remoteIndex = index;
        }

        public string GetChatAtIndex(byte index) => buffer.TryGetValue(index, out string result) ? result : null;
    }
}
