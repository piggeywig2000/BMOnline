using System;

namespace BMOnline.Mod.Chat
{
    public interface IChatManager
    {
        /// <summary>
        /// True if the chat is currently open.
        /// </summary>
        public bool IsOpen { get; }

        /// <summary>
        /// Opens the chat.
        /// </summary>
        public void Open();

        /// <summary>
        /// Closes the chat.
        /// </summary>
        public void Close();

        /// <summary>
        /// Adds a message to the chat.
        /// Unity rich text tags can be used for basic formatting, see https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/StyledText.html.
        /// </summary>
        /// <param name="message"></param>
        public void AddChatMessage(string message);

        /// <summary>
        /// Raised when the chat is opened.
        /// </summary>
        public event EventHandler OnChatOpened;

        /// <summary>
        /// Raised when the chat is closed, irrespective of whether a message was sent.
        /// </summary>
        public event EventHandler OnChatClosed;

        /// <summary>
        /// Raised when a message is added to the chat.
        /// Should not be used for handling chat messages received from the server, because the message may not represent a chat message.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> OnChatMessageAdded;

        /// <summary>
        /// Raised when a message is sent by the user, by typing in the chat and pressing enter.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> OnChatMessageSent;
    }

    public class ChatMessageEventArgs : EventArgs
    {
        private readonly IChatMessage chatMessage;

        public ChatMessageEventArgs(IChatMessage message)
        {
            chatMessage = message;
        }

        public string Message => chatMessage.Text;
    }
}
