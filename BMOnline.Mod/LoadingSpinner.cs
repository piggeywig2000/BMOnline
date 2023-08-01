using System;
using Flash2;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod
{
    internal class LoadingSpinner
    {
        class LoadingSpinnerAnimation : MonoBehaviour
        {
            public LoadingSpinnerAnimation() : base() { }
            public LoadingSpinnerAnimation(IntPtr handle) : base(handle) { }

            private Image[] images = null;
            private bool isPlaying = false;
            private float timeSincePlay = 0;

            public void Start()
            {
                images = new Image[8];
                for (int i = 0; i < 8; i++)
                {
                    images[i] = transform.GetChild(i).GetComponent<Image>();
                }
            }

            public void Update()
            {
                timeSincePlay += Time.unscaledDeltaTime;

                for (int i = 0; i < 8; i++)
                {
                    Image image = images[i];
                    float animTime = (((timeSincePlay * 6) + (8 - i)) % 8) / 6;
                    float newAlpha = Mathf.Max(1 - animTime, 0);
                    newAlpha *= Mathf.Min(timeSincePlay, 1);
                    if (!isPlaying && newAlpha > image.color.a)
                        newAlpha = 0;
                    if (newAlpha != image.color.a)
                        image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                }
            }

            public void Play()
            {
                isPlaying = true;
                timeSincePlay = 0;
            }

            public void Pause()
            {
                isPlaying = false;
            }
        }

        private readonly LoadingSpinnerAnimation loadingSpinner = null;

        public LoadingSpinner()
        {
            ClassInjector.RegisterTypeInIl2Cpp<LoadingSpinnerAnimation>();
            GameObject spinnerGo = UnityEngine.Object.Instantiate(AssetBundleItems.LoadingPrefab, AppSystemUI.Instance.transform.Find("UIList_GUI_Front").Find("c_system_0").Find("safe_area"));
            loadingSpinner = spinnerGo.transform.GetChild(0).gameObject.AddComponent<LoadingSpinnerAnimation>();
        }

        public void SetPlaying(bool isPlaying)
        {
            if (isPlaying)
                loadingSpinner.Play();
            else
                loadingSpinner.Pause();
        }
    }
}
