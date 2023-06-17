using UnityEngine;

namespace BMOnline.Mod.Chat
{
    internal class SpamTracker
    {
        private KeyCode key;
        private float holdTimer = 0;
        private float spamTimer = 0;

        public SpamTracker(KeyCode key)
        {
            this.key = key;
        }

        public bool UpdateAndGetState()
        {
            if (Input.GetKey(key))
            {
                holdTimer += Time.unscaledDeltaTime;
                bool isSpamming = false;
                if (holdTimer >= 0.5f)
                {
                    spamTimer += Time.unscaledDeltaTime;
                    if (spamTimer >= 0.025f)
                    {
                        spamTimer -= 0.025f;
                        isSpamming = true;
                    }
                }
                return Input.GetKeyDown(key) || isSpamming;
            }
            else
            {
                holdTimer = 0;
                spamTimer = 0;
                return false;
            }
        }
    }
}
