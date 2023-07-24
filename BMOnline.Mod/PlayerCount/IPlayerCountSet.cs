using System.Collections.Generic;
using Flash2;

namespace BMOnline.Mod.PlayerCount
{
    internal interface IPlayerCountSet<out T> where T : class
    {
        void RecreateItemsIfNeeded();
        void DestroyAllItems();
        void UpdateText(PlayerCountData counts);
    }
}
