using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BMOnline.Common;
using UnityEngine;

namespace BMOnline.Mod
{
    internal class ModSettings
    {

        public ModSettings(Dictionary<string, object> settings)
        {
            Log.Info("Loading configuration");
            //Get server address
            try
            {
                if (settings.TryGetValue("ServerIP", out object objIp) && objIp is string strIp && !string.IsNullOrWhiteSpace(strIp))
                {
                    if (IPAddress.TryParse(strIp, out IPAddress ip))
                        ServerIpAddress = ip;
                    else
                    {
                        ServerIpAddress = Dns.GetHostAddresses(strIp).FirstOrDefault();
                    }
                }
            }
            catch (Exception) { }
            //If no IP address specified use piggeywig2000.com, or localhost if DNS don't work for some reason
#if !DEBUG
            ServerIpAddress ??= Dns.GetHostAddresses("piggeywig2000.com").FirstOrDefault();
#endif
            ServerIpAddress ??= IPAddress.Loopback;
            if (settings.TryGetValue("ServerPort", out object objPort) && objPort is string strPort && !string.IsNullOrWhiteSpace(strPort) && ushort.TryParse(strPort, out ushort port))
            {
                ServerPort = port;
            }
            if (settings.TryGetValue("ServerPassword", out object objPassword) && objPassword is string pasword && !string.IsNullOrWhiteSpace(pasword))
            {
                ServerPassword = pasword;
            }
            Log.Config($"Server IP: {ServerIpAddress}    Server Port: {ServerPort}    Password Provided: {(ServerPassword != null ? "Yes" : "No")}");
            //Get settings
            if (settings.TryGetValue("ShowNameTags", out object objShowNameTags) && objShowNameTags is bool showNameTags)
            {
                this.showNameTags = showNameTags;
            }
            Log.Config(ShowNameTags ? "Name Tags: Visible" : "Name Tags: Hidden");
            if (settings.TryGetValue("ShowPlayerCounts", out object objShowPlayerCounts) && objShowPlayerCounts is bool showPlayerCounts)
            {
                this.showPlayerCounts = showPlayerCounts;
            }
            Log.Config(ShowPlayerCounts ? "Player Counts: Visible" : "Player Counts: Hidden");
            if (settings.TryGetValue("EnableChat", out object objEnableChat) && objEnableChat is bool enableChat)
            {
                this.enableChat = enableChat;
            }
            Log.Config(EnableChat ? "Chat: Enabled" : "Chat: Disabled");
        }

        public IPAddress ServerIpAddress { get; private set; } = null;
        public ushort ServerPort { get; private set; } = 10998;
        public string ServerPassword { get; private set; } = null;

        private bool showNameTags = true;
        public bool ShowNameTags
        {
            get => showNameTags;
            set
            {
                showNameTags = value;
                OnSettingChanged?.Invoke(this, new OnSettingChangedEventArgs(Setting.ShowNameTags));
            }
        }

        private bool showPlayerCounts = true;
        public bool ShowPlayerCounts
        {
            get => showPlayerCounts;
            set
            {
                showPlayerCounts = value;
                OnSettingChanged?.Invoke(this, new OnSettingChangedEventArgs(Setting.ShowPlayerCounts));
            }
        }

        private bool enableChat = true;
        public bool EnableChat
        {
            get => enableChat;
            set
            {
                enableChat = value;
                OnSettingChanged?.Invoke(this, new OnSettingChangedEventArgs(Setting.EnableChat));
            }
        }

        public enum Setting
        {
            ServerIpAddress,
            ServerPort,
            ServerPassword,
            ShowNameTags,
            ShowPlayerCounts,
            EnableChat
        }
        public class OnSettingChangedEventArgs : EventArgs
        {
            public OnSettingChangedEventArgs(Setting settingChanged)
            {
                SettingChanged = settingChanged;
            }

            public Setting SettingChanged { get; }
        }
        public event EventHandler<OnSettingChangedEventArgs> OnSettingChanged;

        public void CheckHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ShowNameTags = !ShowNameTags;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ShowPlayerCounts = !ShowPlayerCounts;
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                EnableChat = !EnableChat;
            }
        }
    }
}
