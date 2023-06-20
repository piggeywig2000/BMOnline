using Flash2;
using UnityEngine;
using UnityEngine.UI;
using static BMOnline.Mod.ModSettings;

namespace BMOnline.Mod
{
    internal class NotificationsManager
    {
        private const float FLY_TIME = 0.25f;
        private const float VISIBLE_TIME = 5f;

        private readonly GameObject root;
        private readonly RectTransform containerTransform;
        private readonly Text textObject;

        private float timeSinceAnimStart = 0;
        private float animLength = 0;

        public NotificationsManager(ModSettings settings)
        {
            root = Object.Instantiate(AssetBundleItems.NotificationPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").transform.Find("c_system_0").Find("safe_area"));
            containerTransform = root.transform.Find("Background").GetComponent<RectTransform>();
            textObject = containerTransform.GetComponentInChildren<Text>();

            settings.OnSettingChanged += (s, e) =>
            {
                switch (e.SettingChanged)
                {
                    case Setting.ShowNameTags:
                        ShowNotification(settings.ShowNameTags ? "Name Tags: Visible" : "Name Tags: Hidden");
                        return;
                    case Setting.NameTagSize:
                        ShowNotification($"Name Tag Size: {settings.NameTagSize}");
                        return;
                    case Setting.ShowPlayerCounts:
                        ShowNotification(settings.ShowPlayerCounts ? "Player Counts: Visible" : "Player Counts: Hidden");
                        return;
                    case Setting.EnableChat:
                        ShowNotification(settings.EnableChat ? "Chat: Enabled" : "Chat: Disabled");
                        return;
                    case Setting.PlayerVisibility:
                        ShowNotification(settings.PlayerVisibility == PlayerVisibilityOption.ShowAll ? "Players: Visible" : (settings.PlayerVisibility == PlayerVisibilityOption.HideNear ? "Players: Nearby Hidden" : "Players: Hidden"));
                        return;
                    case Setting.PersonalSpace:
                        ShowNotification($"Personal Space: {settings.PersonalSpace:0.#}");
                        return;
                }
            };
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
F3: Toggle player counts visibility
F4: Toggle chat visibility
F5: Toggle player visibility", 10);
            }
        }

        /// <summary>
        /// Show the notification text for a certain amount of time
        /// </summary>
        /// <param name="text">The notification text. The text will not automatically wrap, so include newlines if the text is long.</param>
        /// <param name="visibleTime">How long the notification is shown for, in seconds, including the flyin animation.</param>
        public void ShowNotification(string text, float visibleTime = VISIBLE_TIME)
        {
            textObject.text = text;
            timeSinceAnimStart = 0;
            animLength = visibleTime;
        }
    }
}
