using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BMOnline.Common;
using BMOnline.Mod.Chat;
using UnityEngine;

namespace BMOnline.Mod.Settings
{
    internal class BmoSettings : IBmoSettings
    {
        private readonly SpamTracker minusSpam;
        private readonly SpamTracker plusSpam;

        public BmoSettings(Dictionary<string, object> settings)
        {
            minusSpam = new SpamTracker(KeyCode.Minus);
            plusSpam = new SpamTracker(KeyCode.Equals);

            Log.Info("Loading configuration");
            //Get server address
            try
            {
                if (settings.TryGetValue("ServerIP", out object objIp) && objIp is string strIp && !string.IsNullOrWhiteSpace(strIp))
                {
                    if (IPAddress.TryParse(strIp, out IPAddress ip))
                        serverIpAddress = new ReadOnlyBmoSetting<IPAddress>(ip);
                    else
                        serverIpAddress = new ReadOnlyBmoSetting<IPAddress>(Dns.GetHostAddresses(strIp).FirstOrDefault());
                }
            }
            catch (Exception) { }
            //If no IP address specified use piggeywig2000.com, or localhost if DNS don't work for some reason
#if !DEBUG
            serverIpAddress ??= new ReadOnlyBmoSetting<IPAddress>(Dns.GetHostAddresses("piggeywig2000.com").FirstOrDefault());
#endif
            serverIpAddress ??= new ReadOnlyBmoSetting<IPAddress>(IPAddress.Loopback);
            if (settings.TryGetValue("ServerPort", out object objPort) && objPort is string strPort && !string.IsNullOrWhiteSpace(strPort) && ushort.TryParse(strPort, out ushort port))
            {
                serverPort = new ReadOnlyBmoSetting<ushort>(port);
            }
            serverPort ??= new ReadOnlyBmoSetting<ushort>(10998);
            if (settings.TryGetValue("ServerPassword", out object objPassword) && objPassword is string password && !string.IsNullOrWhiteSpace(password))
            {
                serverPassword = new ReadOnlyBmoSetting<string>(password);
            }
            serverPassword ??= new ReadOnlyBmoSetting<string>(null);
            Log.Config($"Server IP: {ServerIpAddress}    Server Port: {ServerPort}    Password Provided: {(ServerPassword.Value != null ? "Yes" : "No")}");

            //ShowNameTags
            if (settings.TryGetValue("ShowNameTags", out object objShowNameTags) && objShowNameTags is bool showNameTags)
            {
                this.showNameTags = new BmoSetting<bool>(showNameTags);
            }
            this.showNameTags ??= new BmoSetting<bool>(true);
            Log.Config(ShowNameTags.Value ? "Name Tags: Visible" : "Name Tags: Hidden");

            //NameTagSize
            Func<int, int> nameTagSizeTransformation = new Func<int, int>((size) => Math.Max(size, 1));
            if (settings.TryGetValue("NameTagSize", out object objNameTagSize) && objNameTagSize is float nameTagSize)
            {
                this.nameTagSize = new TransformativeBmoSetting<int>((int)nameTagSize, nameTagSizeTransformation);
            }
            this.nameTagSize ??= new TransformativeBmoSetting<int>(48, nameTagSizeTransformation);
            Log.Config($"Name Tag Size: {NameTagSize}");

            //ShowPlayerCounts
            if (settings.TryGetValue("ShowPlayerCounts", out object objShowPlayerCounts) && objShowPlayerCounts is bool showPlayerCounts)
            {
                this.showPlayerCounts = new BmoSetting<bool>(showPlayerCounts);
            }
            this.showPlayerCounts ??= new BmoSetting<bool>(true);
            Log.Config(ShowPlayerCounts.Value ? "Player Counts: Visible" : "Player Counts: Hidden");

            //EnableChat
            if (settings.TryGetValue("EnableChat", out object objEnableChat) && objEnableChat is bool enableChat)
            {
                this.enableChat = new BmoSetting<bool>(enableChat);
            }
            this.enableChat ??= new BmoSetting<bool>(true);
            Log.Config(EnableChat.Value ? "Chat: Enabled" : "Chat: Disabled");

            //PlayerVisibility
            if (settings.TryGetValue("PlayerVisibility", out object objPlayerVisibility) && objPlayerVisibility is string playerVisibility && !string.IsNullOrWhiteSpace(playerVisibility))
            {
                PlayerVisibilityOption visibility = PlayerVisibilityOption.ShowAll;
                switch (playerVisibility.ToLower())
                {
                    case "showall":
                        visibility = PlayerVisibilityOption.ShowAll;
                        break;
                    case "hidenear":
                        visibility = PlayerVisibilityOption.HideNear;
                        break;
                    case "hideall":
                        visibility = PlayerVisibilityOption.HideAll;
                        break;
                }
                this.playerVisibility = new BmoSetting<PlayerVisibilityOption>(visibility);
            }
            this.playerVisibility ??= new BmoSetting<PlayerVisibilityOption>(PlayerVisibilityOption.ShowAll);
            Log.Config($"Player Visibility: {PlayerVisibility:g}");

            //PersonalSpace
            Func<float, float> personalSpaceTransformation = new Func<float, float>((space) => Math.Max(Mathf.Round(space * 10) / 10, 0.1f));
            if (settings.TryGetValue("PersonalSpace", out object objPersonalSpace) && objPersonalSpace is float personalSpace)
            {
                this.personalSpace = new TransformativeBmoSetting<float>(personalSpace, personalSpaceTransformation);
            }
            this.personalSpace ??= new TransformativeBmoSetting<float>(2, personalSpaceTransformation);
            Log.Config($"Personal Space: {PersonalSpace:0.#}");
        }

        private readonly ReadOnlyBmoSetting<IPAddress> serverIpAddress;
        public IReadOnlyBmoSetting<IPAddress> ServerIpAddress => serverIpAddress;

        private readonly ReadOnlyBmoSetting<ushort> serverPort;
        public IReadOnlyBmoSetting<ushort> ServerPort => serverPort;

        private readonly ReadOnlyBmoSetting<string> serverPassword;
        public IReadOnlyBmoSetting<string> ServerPassword => serverPassword;

        private readonly BmoSetting<bool> showNameTags;
        public IBmoSetting<bool> ShowNameTags => showNameTags;

        private readonly TransformativeBmoSetting<int> nameTagSize;
        public IBmoSetting<int> NameTagSize => nameTagSize;

        private readonly BmoSetting<bool> showPlayerCounts;
        public IBmoSetting<bool> ShowPlayerCounts => showPlayerCounts;

        private readonly BmoSetting<bool> enableChat;
        public IBmoSetting<bool> EnableChat => enableChat;

        private readonly BmoSetting<PlayerVisibilityOption> playerVisibility;
        public IBmoSetting<PlayerVisibilityOption> PlayerVisibility => playerVisibility;

        private readonly TransformativeBmoSetting<float> personalSpace;
        public IBmoSetting<float> PersonalSpace => personalSpace;

        public void Update()
        {
            if (ShowNameTags.IsHotkeyEnabled && Input.GetKeyDown(KeyCode.F2))
            {
                ShowNameTags.SetValue(!ShowNameTags.Value);
            }
            if (ShowPlayerCounts.IsHotkeyEnabled && Input.GetKeyDown(KeyCode.F3))
            {
                ShowPlayerCounts.SetValue(!ShowPlayerCounts.Value);
            }
            if (EnableChat.IsHotkeyEnabled && Input.GetKeyDown(KeyCode.F4))
            {
                EnableChat.SetValue(!EnableChat.Value);
            }
            if (PlayerVisibility.IsHotkeyEnabled && Input.GetKeyDown(KeyCode.F5))
            {
                PlayerVisibility.SetValue((PlayerVisibilityOption)(((int)PlayerVisibility.Value + 1) % 3));
            }

            bool isMinus = minusSpam.UpdateAndGetState();
            bool isPlus = plusSpam.UpdateAndGetState();
            if (isMinus || isPlus)
            {
                if (NameTagSize.IsHotkeyEnabled && ShowNameTags.IsHotkeyEnabled && Input.GetKey(KeyCode.F2))
                {
                    ShowNameTags.SetValue(true);
                    NameTagSize.SetValue(NameTagSize.Value + (isPlus ? 1 : -1));
                }
                else if (PersonalSpace.IsHotkeyEnabled && PlayerVisibility.IsHotkeyEnabled && Input.GetKey(KeyCode.F5))
                {
                    PlayerVisibility.SetValue(PlayerVisibilityOption.HideNear);
                    PersonalSpace.SetValue(PersonalSpace.Value + (isPlus ? 0.1f : -0.1f));
                }
            }
        }
    }
}
