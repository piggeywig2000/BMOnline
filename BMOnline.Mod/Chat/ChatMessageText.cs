namespace BMOnline.Mod.Chat
{
    internal class ChatMessageText : IChatMessage
    {
        public ChatMessageText(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
