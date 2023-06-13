using Flash2;
using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod
{
    internal class NetPlayer
    {
        public NetPlayer(ushort id, string name)
        {
            Id = id;
            Name = name;
        }

        public ushort Id { get; }
        public string Name { get; }
        public Chara.eKind Character { get; set; }
        public byte SkinIndex { get; set; }
        public CharaCustomize.PartsSet Customisations { get; set; }
        public GameObject GameObject { get; private set; }
        public Transform BehaviourTransform { get; private set; }
        public Transform PhysicalTransform { get; private set; }
        public Player PlayerObject { get; private set; }
        public GravityTilt GravityTilt { get; private set; }
        public PlayerMotion Motion { get; private set; }
        public GameObject NameTag { get; private set; }
        public Vector3 Position { get; set; } = Vector3.zero;
        public Vector3 AngularVelocity { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public Vector3 Velocity { get; set; } = Vector3.zero;
        public PlayerMotion.State MotionState { get; set; } = PlayerMotion.State.IDLE;
        public bool IsOnGround { get; set; } = false;

        public void Instantiate(Transform root, MainGameStage mainGameStage, bool showNametag)
        {
            GameObject playerPrefab = AssetBundleCache.Instance.getAsset<GameObject>("Player/Common/Ghost.prefab");
            GameObject = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, root);
            GameObject.name = "OnlinePlayer";
            PlayerObject = GameObject.GetComponentInChildren<Player>();
            PlayerObject.Initialize(new Chara.SelectDatum() { m_CharaKind = Character, m_SkinIndex = SkinIndex, m_PartsSetIndex = 0 }, mainGameStage.gameObject, new Vector3(0, 0, 0), Quaternion.identity, 0, true, Customisations);

            BehaviourTransform = PlayerObject.transform;
            PhysicalTransform = GameObject.transform.Find("GhostPhysical");

            CharaCustomizeAssignBallShape ballShape = BehaviourTransform.GetComponent<CharaCustomizeAssignBallShape>();
            ballShape.InstantiateBallShape(Customisations);

            GameObject.Destroy(PhysicalTransform.GetComponent<ReplayTracker>());

            GravityTilt = BehaviourTransform.GetComponent<GravityTilt>();
            GravityTilt.m_UpdateMode = (GravityTilt.UpdateMode)4; //This should prevent it automatically updating itself

            //I think this is meant to happen when you register it with PhysicsBallManager, but we're not doing that so let's just set it manually
            PlayerObject.SetPhysicsBall(PhysicalTransform.GetComponent<MainGameGhostBall>());

            PlayerObject.setState(null);

            Motion = PlayerObject.GetMotion();
            //Destroying the reference so that Player won't call PlayerMotion's update function. Hopefully this doesn't break anything
            PlayerObject.m_PlayerMotion = null;
            Motion.OnStart(PlayerObject.gameObject);

            //Create nametag
            if (showNametag)
            {
                NameTag = GameObject.Instantiate(AssetBundleItems.NameTagPrefab, new Vector3(0, 0, 0), Quaternion.identity, GameObject.transform);
                NameTag.GetComponentInChildren<Text>().text = Name;
            }

            Reset(mainGameStage);
        }

        public void Reset(MainGameStage mainGameStage)
        {
            GravityTilt.Initialize(mainGameStage.GetPlayer().gameObject, PhysicalTransform);
            PhysicalTransform.GetComponent<MainGameGhostBall>().SetReplayState();
        }
    }
}
