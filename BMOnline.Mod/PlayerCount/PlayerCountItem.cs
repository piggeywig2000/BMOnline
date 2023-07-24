using System;
using Flash2;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.PlayerCount
{
    internal class PlayerCountItem<T> where T : class
    {
        private readonly GameObject root;
        private readonly SelScrollRectCellBase<T, SelDiagonalScrollRectContext> cellData;
        private readonly Func<T, PlayerCountData, int> getPlayerCountFunc;
        private readonly GameObject countRoot;
        private readonly Text countText;
        private int lastValue;

        public PlayerCountItem(GameObject root, Func<T, PlayerCountData, int> getPlayerCountFunc)
        {
            this.root = root;
            cellData = root.GetComponent<SelScrollRectCellBase<T, SelDiagonalScrollRectContext>>();
            this.getPlayerCountFunc = getPlayerCountFunc;
            //Instantiate player count graphic
            countRoot = UnityEngine.Object.Instantiate(AssetBundleItems.PlayerCountPrefab, root.transform);
            countRoot.transform.localPosition = new Vector3(-449, 0, 0);
            countRoot.transform.localRotation = Quaternion.identity;
            countRoot.transform.localScale = Vector3.one;
            countText = countRoot.GetComponentInChildren<Text>();
            lastValue = -1;
        }

        public bool IsRootDestroyed => root == null;

        public void Destroy()
        {
            UnityEngine.Object.Destroy(countRoot);
        }

        public void UpdateText(PlayerCountData counts)
        {
            int playerCount = cellData?.itemData != null ? getPlayerCountFunc(cellData.itemData, counts) : 0;
            bool active = playerCount != 0;
            if (active != countRoot.activeSelf)
                countRoot.SetActive(active);
            if (playerCount != lastValue && countRoot.activeInHierarchy)
            {
                countText.text = playerCount.ToString();
                lastValue = playerCount;
            }
        }
    }
}
