namespace BMOnline.Mod.Settings
{
    internal class ReadOnlyBmoSetting<T> : IReadOnlyBmoSetting<T>
    {
        protected T value;

        public ReadOnlyBmoSetting(T value)
        {
            this.value = value;
            IsHotkeyEnabled = true;
        }

        public T Value { get => value; }

        public bool IsHotkeyEnabled { get; set; }

        public override string ToString() => Value?.ToString();
    }
}
