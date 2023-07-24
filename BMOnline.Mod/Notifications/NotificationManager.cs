using BMOnline.Mod.Settings;
using Flash2;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.Notifications
{
    internal class NotificationManager : INotificationManager
    {
        private const float FLY_TIME = 0.25f;
        private const float VISIBLE_TIME = 5f;

        private readonly GameObject root;
        private readonly RectTransform containerTransform;
        private readonly Text textObject;

        private float timeSinceAnimStart = 0;
        private float animLength = 0;

        public NotificationManager(IBmoSettings settings)
        {
            root = Object.Instantiate(AssetBundleItems.NotificationPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").transform.Find("c_system_0").Find("safe_area"));
            containerTransform = root.transform.Find("Background").GetComponent<RectTransform>();
            textObject = containerTransform.GetComponentInChildren<Text>();

            settings.ShowNameTags.OnChanged += (s, e) => { ShowNotification(settings.ShowNameTags.Value ? "Name Tags: Visible" : "Name Tags: Hidden"); };
            settings.NameTagSize.OnChanged += (s, e) => { ShowNotification($"Name Tag Size: {settings.NameTagSize}"); };
            settings.PlayerCountMode.OnChanged += (s, e) => { ShowNotification(settings.PlayerCountMode.Value == PlayerCountOption.Mixed ? "Player Count Mode: Mixed" : settings.PlayerCountMode.Value == PlayerCountOption.ExactMode ? "Player Count Mode: Exact Mode" : settings.PlayerCountMode.Value == PlayerCountOption.SumOfStages ? "Player Count Mode: Sum Of Stages" : "Player Count Mode: Disabled"); };
            settings.EnableChat.OnChanged += (s, e) => { ShowNotification(settings.EnableChat.Value ? "Chat: Enabled" : "Chat: Disabled"); };
            settings.PlayerVisibility.OnChanged += (s, e) => { ShowNotification(settings.PlayerVisibility.Value == PlayerVisibilityOption.ShowAll ? "Players: Visible" : settings.PlayerVisibility.Value == PlayerVisibilityOption.HideNear ? "Players: Nearby Hidden" : "Players: Hidden"); };
            settings.PersonalSpace.OnChanged += (s, e) => { ShowNotification($"Personal Space: {settings.PersonalSpace:0.#}"); };
        }

        /// <summary>
        /// Updates the animation
        /// </summary>
        public void Update()
        {
            timeSinceAnimStart += Time.unscaledDeltaTime;
            bool isVisible = timeSinceAnimStart < animLength;

            if (root.activeSelf != isVisible)
                root.SetActive(isVisible);

            if (isVisible)
            {
                float timeSinceFlyOutStart = timeSinceAnimStart - (animLength - FLY_TIME);
                containerTransform.localPosition = new Vector3(
                    timeSinceFlyOutStart < 0 ?
                        Mathf.Lerp(textObject.preferredWidth + 20, 0, timeSinceAnimStart / FLY_TIME) :
                        Mathf.Lerp(0, textObject.preferredWidth + 20, timeSinceFlyOutStart / FLY_TIME),
                    containerTransform.localPosition.y,
                    containerTransform.localPosition.z);
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                ShowNotification(@"Keybinds:
T: Open the chat
F1: Show keybinds
F2: Toggle name tag visibility
F2 and +/-: Adjust name tag size
F3: Change player count mode
F4: Toggle chat visibility
F5: Change player visibility
F5 and +/-: Adjust personal space", 10);
            }
        }

        public void ShowNotification(string text) => ShowNotification(text, VISIBLE_TIME);

        public void ShowNotification(string text, float visibleTime)
        {
            textObject.text = text;
            timeSinceAnimStart = 0;
            animLength = visibleTime;
        }
    }
}
