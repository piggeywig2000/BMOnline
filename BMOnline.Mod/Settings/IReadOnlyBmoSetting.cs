namespace BMOnline.Mod.Settings
{
    public interface IReadOnlyBmoSetting<T>
    {
        /// <summary>
        /// The current value of this setting.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Whether the value of this setting can be changed using its hotkey, if it can be changed with a hotkey.
        /// </summary>
        public bool IsHotkeyEnabled { get; set; }
    }
}
