namespace BMOnline.Mod.Notifications
{
    public interface INotificationManager
    {
        /// <summary>
        /// Show the notification text for the default length of time.
        /// </summary>
        /// <param name="text">The notification text. The text will not automatically wrap, so include newlines if the text is long.</param>
        public void ShowNotification(string text);

        /// <summary>
        /// Show the notification text for a specified amount of time.
        /// </summary>
        /// <param name="text">The notification text. The text will not automatically wrap, so include newlines if the text is long.</param>
        /// <param name="visibleTime">How long the notification is shown for, in seconds, including the fly-in and fly-out animation.</param>
        public void ShowNotification(string text, float visibleTime);
    }
}
