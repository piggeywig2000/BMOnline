using System;

namespace BMOnline.Mod.Settings
{
    internal class BmoSetting<T> : ReadOnlyBmoSetting<T>, IBmoSetting<T>
    {
        public BmoSetting(T value) : base(value)
        {
        }

        public event EventHandler OnChanged;

        public virtual void SetValue(T value)
        {
            T oldValue = this.value;
            this.value = value;
            if (!value.Equals(oldValue))
            {
                OnChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
