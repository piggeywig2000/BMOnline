using System.Net;

namespace BMOnline.Mod.Settings
{
    public interface IBmoSettings
    {
        public IReadOnlyBmoSetting<IPAddress> ServerIpAddress { get; }
        public IReadOnlyBmoSetting<ushort> ServerPort { get; }
        public IReadOnlyBmoSetting<string> ServerPassword { get; }
        public IBmoSetting<bool> ShowNameTags { get; }
        public IBmoSetting<int> NameTagSize { get; }
        public IBmoSetting<PlayerCountOption> PlayerCountMode { get; }
        public IBmoSetting<bool> EnableChat { get; }
        public IBmoSetting<PlayerVisibilityOption> PlayerVisibility { get; }
        public IBmoSetting<float> PersonalSpace { get; }
    }

    public enum PlayerCountOption
    {
        Mixed,
        ExactMode,
        SumOfStages,
        Disabled
    }

    public enum PlayerVisibilityOption
    {
        ShowAll,
        HideNear,
        HideAll
    }
}
