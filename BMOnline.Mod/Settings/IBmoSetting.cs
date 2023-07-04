using System;

namespace BMOnline.Mod.Settings
{
    public interface IBmoSetting<T> : IReadOnlyBmoSetting<T>
    {
        /// <summary>
        /// Changes the value of this setting.
        /// </summary>
        /// <param name="value">The value to set this setting to.</param>
        public void SetValue(T value);

        /// <summary>
        /// Raised when the value of this setting is changed.
        /// </summary>
        public event EventHandler OnChanged;
    }
}
