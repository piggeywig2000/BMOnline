﻿using UnityEngine;
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
