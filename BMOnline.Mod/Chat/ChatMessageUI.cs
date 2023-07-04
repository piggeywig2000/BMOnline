using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.Chat
{
    internal class ChatMessageUI : IChatMessage
    {
        private readonly CanvasGroup canvasGroup;
        private float timeAlive = 0;

        public string Text { get; }

        public ChatMessageUI(string messageText, Transform parent)
        {
            Text = messageText;
            GameObject chatGameObject = GameObject.Instantiate(AssetBundleItems.ChatMessagePrefab, parent);
            chatGameObject.GetComponentInChildren<Text>().text = Text;
            canvasGroup = chatGameObject.GetComponent<CanvasGroup>();
        }

        public void Update(bool isChatOpen)
        {
            timeAlive += Time.unscaledDeltaTime;
            float newAlpha = isChatOpen ? 1 : Mathf.Lerp(1, 0, timeAlive - 10);
            if (canvasGroup.alpha != newAlpha)
            {
                canvasGroup.alpha = newAlpha;
                canvasGroup.gameObject.SetActive(newAlpha > 0);
            }
        }

        public void Destroy() => GameObject.Destroy(canvasGroup.gameObject);
    }
}
