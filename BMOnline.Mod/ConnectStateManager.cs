using Flash2;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod
{
    internal class ConnectStateManager
    {
        private readonly GameObject root;
        private readonly GameObject gameobjectConnected;
        private readonly GameObject gameobjectConnecting;
        private readonly GameObject gameobjectDisconnected;
        private readonly Text subtitleText;

        private State state;
        private string subtitle = "";

        public ConnectStateManager()
        {
            root = GameObject.Instantiate(AssetBundleItems.ConnectStatusPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").Find("c_system_0").Find("safe_area"));
            Transform container = root.transform.Find("Container");
            gameobjectConnected = container.Find("Connected").gameObject;
            gameobjectConnecting = container.Find("Connecting").gameObject;
            gameobjectDisconnected = container.Find("Disconnected").gameObject;
            subtitleText = container.Find("Subtitle").GetComponent<Text>();
            state = State.Hidden;
        }

        public void SetVisibility(bool visible)
        {
            if (root.activeSelf != visible)
                root.SetActive(visible);
        }

        private void SetSubtitle(string subtitle)
        {
            if (this.subtitle != subtitle)
            {
                subtitleText.text = subtitle;
                this.subtitle = subtitle;
            }
        }

        public void SetConnected(int playersOnline)
        {
            if (state != State.Connected)
            {
                state = State.Connected;
                gameobjectConnecting.SetActive(false);
                gameobjectDisconnected.SetActive(false);
                gameobjectConnected.SetActive(true);
            }

            SetSubtitle($"Players online: {playersOnline}");
        }

        public void SetConnecting()
        {
            if (state != State.Connecting)
            {
                state = State.Connecting;
                gameobjectConnected.SetActive(false);
                gameobjectDisconnected.SetActive(false);
                gameobjectConnecting.SetActive(true);
            }

            SetSubtitle("");
        }

        public void SetDisconnected(string reason)
        {
            if (state != State.Disconnected)
            {
                state = State.Disconnected;
                gameobjectConnected.SetActive(false);
                gameobjectConnecting.SetActive(false);
                gameobjectDisconnected.SetActive(true);
            }

            SetSubtitle(reason);
        }

        private enum State
        {
            Hidden,
            Connected,
            Connecting,
            Disconnected
        }
    }
}
