using System.IO;
using System.Reflection;
using UnityEngine;

namespace BMOnline.Mod
{
    internal static class AssetBundleItems
    {
        private static string dllFolder;
        private static string DllFolder
        {
            get
            {
                if (dllFolder == null)
                {
                    dllFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return dllFolder;
            }
        }

        private static AssetBundle bmonlineAb;
        private static AssetBundle BmOnlineAb
        {
            get
            {
                if (bmonlineAb == null)
                {
                    bmonlineAb = AssetBundle.LoadFromFile(Path.Combine(DllFolder, "bmonline"));
                }
                return bmonlineAb;
            }
        }

        private static GameObject nameTagPrefab;
        public static GameObject NameTagPrefab
        {
            get
            {
                if (nameTagPrefab == null)
                {
                    nameTagPrefab = BmOnlineAb.LoadAsset<GameObject>("Assets/Prefabs/NameTag.prefab");
                }
                return nameTagPrefab;
            }
        }

        private static GameObject playerCountPrefab;
        public static GameObject PlayerCountPrefab
        {
            get
            {
                if (playerCountPrefab == null)
                {
                    playerCountPrefab = BmOnlineAb.LoadAsset<GameObject>("Assets/Prefabs/PlayerCount.prefab");
                }
                return playerCountPrefab;
            }
        }

        private static GameObject connectStatusPrefab;
        public static GameObject ConnectStatusPrefab
        {
            get
            {
                if (connectStatusPrefab == null)
                {
                    connectStatusPrefab = BmOnlineAb.LoadAsset<GameObject>("Assets/Prefabs/ConnectStatus.prefab");
                }
                return connectStatusPrefab;
            }
        }

        private static GameObject chatPrefab;
        public static GameObject ChatPrefab
        {
            get
            {
                if (chatPrefab == null)
                {
                    chatPrefab = BmOnlineAb.LoadAsset<GameObject>("Assets/Prefabs/Chat.prefab");
                }
                return chatPrefab;
            }
        }

        private static GameObject chatMessagePrefab;
        public static GameObject ChatMessagePrefab
        {
            get
            {
                if (chatMessagePrefab == null)
                {
                    chatMessagePrefab = BmOnlineAb.LoadAsset<GameObject>("Assets/Prefabs/ChatMessage.prefab");
                }
                return chatMessagePrefab;
            }
        }
    }
}
