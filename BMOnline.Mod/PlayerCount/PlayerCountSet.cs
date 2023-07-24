using System;
using System.Linq;
using UnityEngine;

namespace BMOnline.Mod.PlayerCount
{
    internal class PlayerCountSet<T> : IPlayerCountSet<T> where T : class
    {
        private readonly Transform uiList;
        private GameObject itemContainer;
        private readonly Func<T, PlayerCountData, int> getPlayerCountFunc;
        private PlayerCountItem<T>[] items;

        public PlayerCountSet(Transform uiList, string key, Func<T, PlayerCountData, int> getPlayerCountFunc)
        {
            Key = key;
            this.uiList = uiList;
            itemContainer = null;
            this.getPlayerCountFunc = getPlayerCountFunc;
            items = Array.Empty<PlayerCountItem<T>>();
        }

        public string Key { get; }

        public void RecreateItemsIfNeeded()
        {
            //Try to find container if it has been destroyed
            if (itemContainer == null)
            {
                //Look for itemContainer
                itemContainer = uiList.Find(Key)?.Find("safe_area")?.Find("root")?.Find("00")?.Find("ScrollView")?.Find("Viewport")?.Find("Content")?.gameObject;

                //Clear list if itemContainer is not found
                if (itemContainer == null && items.Length > 0)
                {
                    items = Array.Empty<PlayerCountItem<T>>();
                }
            }

            //If we found container, check if we need to recreate the items
            if (itemContainer != null && (items.Length != itemContainer.transform.childCount || items.Any(i => i.IsRootDestroyed)))
            {
                foreach (PlayerCountItem<T> item in items)
                {
                    item.Destroy();
                }
                items = new PlayerCountItem<T>[itemContainer.transform.childCount];
                for (int i = 0; i < itemContainer.transform.childCount; i++)
                {
                    items[i] = new PlayerCountItem<T>(itemContainer.transform.GetChild(i).gameObject, getPlayerCountFunc);
                }
            }
        }

        public void DestroyAllItems()
        {
            foreach (PlayerCountItem<T> item in items)
            {
                item.Destroy();
            }
            items = Array.Empty<PlayerCountItem<T>>();
        }

        public void UpdateText(PlayerCountData counts)
        {
            if (itemContainer == null || !itemContainer.activeInHierarchy) return;
            foreach (PlayerCountItem<T> item in items)
            {
                item.UpdateText(counts);
            }
        }
    }
}
