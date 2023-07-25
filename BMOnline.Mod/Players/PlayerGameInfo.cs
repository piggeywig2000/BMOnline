using System;
using System.Collections.Generic;
using BMOnline.Common.Relay.Snapshots;
using BMOnline.Mod.Patches;
using BMOnline.Mod.Settings;
using Flash2;
using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace BMOnline.Mod.Players
{
    internal class PlayerGameInfo : IPlayerGameInfo
    {
        private readonly struct BallRenderer
        {
            public readonly Renderer Renderer;
            public readonly float Alpha;
            public readonly float Glossiness;
            public readonly float GlossMapScale;
            public readonly float Metallic;

            public BallRenderer(Renderer renderer, float alpha, float glossiness, float glossMapScale, float metallic)
            {
                Renderer = renderer;
                Alpha = alpha;
                Glossiness = glossiness;
                GlossMapScale = glossMapScale;
                Metallic = metallic;
            }
        }

        private readonly IBmoSettings settings;
        private readonly Transform behaviourTransform;
        private readonly Transform physicalTransform;
        private readonly GravityTilt gravityTilt;
        private readonly PlayerMotion playerMotion;
        private readonly List<BallRenderer> ballRenderers;

        private bool isActive;
        private Vector3 position;
        private Vector3 velocity;
        private Vector3 angularVelocity;
        private PlayerMotion.State motionState;
        private bool isOnGround;

        public PlayerGameInfo(IBmoSettings settings, string name, Chara.SelectDatum charSelectDatum, CharaCustomize.PartsSet customisations, Transform root, MainGameStage mainGameStage)
        {
            this.settings = settings;

            GameObject playerPrefab = AssetBundleCache.GetAsset<GameObject>("Player/Common/Ghost.prefab");
            RootGameObject = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, root);
            RootGameObject.name = "OnlinePlayer";
            PlayerObject = RootGameObject.GetComponentInChildren<Player>();
            PlayerObject.Initialize(charSelectDatum, mainGameStage.gameObject, new Vector3(0, 0, 0), Quaternion.identity, 0, true, customisations);

            behaviourTransform = PlayerObject.transform;
            physicalTransform = RootGameObject.transform.Find("GhostPhysical");

            CharaCustomizeAssignBallShape ballShape = behaviourTransform.GetComponent<CharaCustomizeAssignBallShape>();
            ballShape.InstantiateBallShape(customisations);

            //Disable MonkeySound to stop playing voice lines
            PlayerObject.monkeySound.enabled = false;

            GameObject.Destroy(physicalTransform.GetComponent<ReplayTracker>());

            gravityTilt = behaviourTransform.GetComponent<GravityTilt>();
            gravityTilt.m_UpdateMode = (GravityTilt.UpdateMode)4; //This should prevent it automatically updating itself

            //I think this is meant to happen when you register it with PhysicsBallManager, but we're not doing that so let's just set it manually
            PlayerObject.SetPhysicsBall(physicalTransform.GetComponent<MainGameGhostBall>());

            PlayerObject.setState(null);

            playerMotion = PlayerObject.GetMotion();
            //Destroying the reference so that Player won't call PlayerMotion's update function. Hopefully this doesn't break anything
            PlayerObject.m_PlayerMotion = null;
            playerMotion.OnStart(PlayerObject.gameObject);

            //Create nametag
            NameTagGameObject = GameObject.Instantiate(AssetBundleItems.NameTagPrefab, new Vector3(0, 0, 0), Quaternion.identity, RootGameObject.transform);
            NameTagText = NameTagGameObject.GetComponentInChildren<Text>();
            NameTagText.text = name;

            Reset(mainGameStage);

            Transform ballTransform = behaviourTransform.transform.Find("ball");
            ballRenderers = new List<BallRenderer>();
            foreach (Renderer renderer in ballTransform.GetComponentsInChildren<Renderer>())
            {
                ballRenderers.Add(new BallRenderer(renderer,
                    renderer.material.color.a,
                    renderer.material.GetFloat("_Glossiness"),
                    renderer.material.GetFloat("_GlossMapScale"),
                    renderer.material.GetFloat("_Metallic")));
            }
        }

        public GameObject RootGameObject { get; }
        public Player PlayerObject { get; }
        public GameObject NameTagGameObject { get; }
        public Text NameTagText { get; }

        public void Destroy() => GameObject.Destroy(RootGameObject);

        public void FixedUpdate()
        {
            PhysicsBall physicsBall = PlayerObject.m_PhysicsBall;
            float deltaTime = Time.fixedDeltaTime * MgTimeScaleManager.TimeScale;
            physicsBall.m_OldPos = position + ((-velocity) * deltaTime);
            physicsBall.m_Pos = position;
        }

        public void Update()
        {
            //Set isOnGround
            PlayerObject.m_IsOnGround = isOnGround;

            //Update monkey animations
            PlayerMotionPatch.PreventSetState = true;
            try
            {
                if (playerMotion.GetState() != motionState)
                {
                    playerMotion.SetState((PlayerMotion.State)((int)motionState | (1 << 20)));
                }

                playerMotion.ResetFloatingSec();
                playerMotion.OnUpdate();
            }
            finally
            {
                PlayerMotionPatch.PreventSetState = false;
            }
        }

        public void LateUpdateFromSnapshot(StagePositionSnapshot snapshot)
        {
            isActive = snapshot != null;
            if (!isActive)
                return;

            position = new Vector3(snapshot.Position.x, snapshot.Position.y, snapshot.Position.z);
            velocity = new Vector3(snapshot.Velocity.x, snapshot.Velocity.y, snapshot.Velocity.z);
            angularVelocity = new Vector3(snapshot.AngularVelocity.x, snapshot.AngularVelocity.y, snapshot.AngularVelocity.z);
            motionState = (PlayerMotion.State)snapshot.MotionState;
            isOnGround = snapshot.IsOnGround;
        }

        private void SetAlpha(float alpha)
        {
            ballRenderers.RemoveAll(r => r.Renderer == null);
            foreach (BallRenderer ballRenderer in ballRenderers)
            {
                ballRenderer.Renderer.material.color = new Color(ballRenderer.Renderer.material.color.r, ballRenderer.Renderer.material.color.g, ballRenderer.Renderer.material.color.b, ballRenderer.Alpha * alpha);
                ballRenderer.Renderer.material.SetFloat("_Glossiness", ballRenderer.Glossiness * alpha);
                ballRenderer.Renderer.material.SetFloat("_GlossMapScale", ballRenderer.GlossMapScale * alpha);
                ballRenderer.Renderer.material.SetFloat("_Metallic", ballRenderer.Metallic * alpha);
            }
            NameTagText.canvasRenderer.SetColor(new Color(NameTagText.canvasRenderer.GetColor().r, NameTagText.canvasRenderer.GetColor().g, NameTagText.canvasRenderer.GetColor().b, alpha));
        }

        public void LateUpdate(MainGameStage mainGameStage)
        {
            if (RootGameObject.activeSelf != isActive)
                RootGameObject.SetActive(isActive);

            physicalTransform.localPosition = position;
            physicalTransform.Rotate(angularVelocity * Time.deltaTime, Space.World);
            gravityTilt.update();
            physicalTransform.localPosition = behaviourTransform.localPosition;

            if (NameTagGameObject.activeSelf != settings.ShowNameTags.Value)
                NameTagGameObject.SetActive(settings.ShowNameTags.Value);

            if (settings.ShowNameTags.Value)
            {
                NameTagGameObject.transform.localPosition = behaviourTransform.localPosition + (MainGame.isViewStage ? new Vector3(0, 3f, 0) : new Vector3(0, 1f, 0));
                Vector3 lookAtPos = MainGame.isViewStage ? MainGame.Instance.m_viewStageCamera.transform.position : (MainGame.isViewPlayer ? MainGame.Instance.m_photoModeCamera.transform.position : mainGameStage.m_CameraController.GetMainCamera().transform.position);
                lookAtPos = new Vector3(lookAtPos.x, NameTagGameObject.transform.position.y, lookAtPos.z);
                NameTagGameObject.transform.LookAt(lookAtPos);
                NameTagGameObject.transform.localScale = MainGame.isViewStage ? new Vector3(4, 4, 4) : Vector3.one;
                if (NameTagText.fontSize != settings.NameTagSize.Value)
                    NameTagText.fontSize = settings.NameTagSize.Value;
            }

            bool isVisible = true;
            float alpha = 1.0f;
            switch (settings.PlayerVisibility.Value)
            {
                case PlayerVisibilityOption.ShowAll:
                    isVisible = true;
                    break;
                case PlayerVisibilityOption.HideNear:
                    isVisible = true;
                    if (!MainGame.isViewStage && !MainGame.isViewPlayer)
                    {
                        //Set visibility based on distance 
                        Vector3 localPlayerPos = mainGameStage.GetPlayer().transform.position;
                        Vector3 cameraPos = mainGameStage.m_CameraController.GetMainCamera().transform.position;
                        Vector3 remotePlayerPos = behaviourTransform.position;
                        float lineLengthSquared = Mathf.Pow(localPlayerPos.x - cameraPos.x, 2) + Mathf.Pow(localPlayerPos.y - cameraPos.y, 2) + Mathf.Pow(localPlayerPos.z - cameraPos.z, 2);
                        float remoteDistance;
                        if (lineLengthSquared == 0)
                            remoteDistance = Mathf.Pow(remotePlayerPos.x - localPlayerPos.x, 2) + Mathf.Pow(remotePlayerPos.y - localPlayerPos.y, 2) + Mathf.Pow(remotePlayerPos.z - localPlayerPos.z, 2);
                        else
                        {
                            float t = ((remotePlayerPos.x - localPlayerPos.x) * (cameraPos.x - localPlayerPos.x) + (remotePlayerPos.y - localPlayerPos.y) * (cameraPos.y - localPlayerPos.y) + (remotePlayerPos.z - localPlayerPos.z) * (cameraPos.z - localPlayerPos.z)) / lineLengthSquared;
                            t = Math.Max(Math.Min(t, 1), 0);
                            remoteDistance =
                                Mathf.Pow(remotePlayerPos.x - (localPlayerPos.x + (t * (cameraPos.x - localPlayerPos.x))), 2) +
                                Mathf.Pow(remotePlayerPos.y - (localPlayerPos.y + (t * (cameraPos.y - localPlayerPos.y))), 2) +
                                Mathf.Pow(remotePlayerPos.z - (localPlayerPos.z + (t * (cameraPos.z - localPlayerPos.z))), 2);
                        }
                        float min = settings.PersonalSpace.Value;
                        float max = min + Mathf.Min(min, 8f);
                        alpha = Mathf.Lerp(0, 1, (Mathf.Sqrt(remoteDistance) - min) / (max - min));
                    }
                    break;
                case PlayerVisibilityOption.HideAll:
                    isVisible = false;
                    break;
            }
            if (RootGameObject.activeSelf != isVisible)
                RootGameObject.SetActive(isVisible);
            SetAlpha(alpha);
        }

        public void Reset(MainGameStage mainGameStage)
        {
            gravityTilt.Initialize(mainGameStage.GetPlayer().gameObject, physicalTransform);
            physicalTransform.GetComponent<MainGameGhostBall>().SetReplayState();
        }
    }
}
