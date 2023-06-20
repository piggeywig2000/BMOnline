using Flash2;
using UnityEngine;
using UnityEngine.UI;

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

        public NotificationsManager()
        {
            root = UnityEngine.Object.Instantiate(AssetBundleItems.NotificationPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").transform.Find("c_system_0").Find("safe_area"));
            containerTransform = root.transform.Find("Background").GetComponent<RectTransform>();
            textObject = containerTransform.GetComponentInChildren<Text>();
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
                        Mathf.Lerp(containerTransform.sizeDelta.x, 0, timeSinceAnimStart / FLY_TIME) :
                        Mathf.Lerp(0, containerTransform.sizeDelta.x, timeSinceFlyOutStart / FLY_TIME),
                    containerTransform.localPosition.y,
                    containerTransform.localPosition.z);
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
