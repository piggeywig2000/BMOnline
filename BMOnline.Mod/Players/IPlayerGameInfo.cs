using Flash2;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.Players
{
    public interface IPlayerGameInfo
    {
        public GameObject RootGameObject { get; }
        public Player PlayerObject { get; }
        public GameObject NameTagGameObject { get; }
        public Text NameTagText { get; }
    }
}
