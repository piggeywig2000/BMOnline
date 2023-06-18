using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.Chat
{
    internal class ChatMessage
    {
        private readonly CanvasGroup canvasGroup;
        private float timeAlive = 0;

        public ChatMessage(string messageText, Transform parent)
        {
            GameObject chatGameObject = GameObject.Instantiate(AssetBundleItems.ChatMessagePrefab, parent);
            chatGameObject.GetComponentInChildren<Text>().text = messageText;
            canvasGroup = chatGameObject.GetComponent<CanvasGroup>();
        }

        public void Update(bool isChatOpen)
        {
            timeAlive += Time.unscaledDeltaTime;
            canvasGroup.alpha = isChatOpen ? 1 : Mathf.Lerp(1, 0, timeAlive - 10);
        }
    }
}
