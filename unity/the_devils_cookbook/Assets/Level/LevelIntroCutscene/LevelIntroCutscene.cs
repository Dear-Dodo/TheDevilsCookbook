using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TDC.Cooking;
using TDC.Core.Manager;
using TDC.Player;
using UnityAsync;
using UnityEngine;
using Utility;
using Object = UnityEngine.Object;

namespace TDC.Level
{
    [Serializable]
    public class LevelIntroCutscene : GameManagerSubsystem
    {
        public PlayerCharacter Player;

        [HideInInspector] public Appliance SoulImbuer;
        [HideInInspector] public Appliance Oven;
        [HideInInspector] public Appliance TrashCan;

        public AsyncManualResetEvent OnComplete = new AsyncManualResetEvent();

        public Sprite Sprite3;
        public Sprite Sprite2;
        public Sprite Sprite1;
        public Sprite SpriteGo;

        public Action OnStart;
        public Action<Sprite> OnTargetSet;
        public Action<Sprite> OnGo;
        public Action OnEnd;

        public float SequenceTime;

        public CameraController ViewingCamera;

        private Transform _CurrentTarget;
        private Sprite _CurrentSprite;

        private List<Sprite> _CountdownSprites = new List<Sprite>();

        protected override Task OnInitialise()
        {
            _CountdownSprites.Add(Sprite3);
            _CountdownSprites.Add(Sprite2);
            _CountdownSprites.Add(Sprite1);

            GameManager.RunOnLevelInitialisationPersistant(OnLevelLoad);

            return Task.CompletedTask;
        }

        private async void OnLevelLoad()
        {
            Appliance[] appliances = GameManager.Appliances;

            foreach (var appliance in appliances)
            {
                appliance.gameObject.SetActive(false);
            }

            foreach (var appliance in appliances)
            {
                if (appliance.GetType() == typeof(Oven))
                {
                    Oven ??= appliance;
                    continue;
                }

                if (appliance.GetType() == typeof(TrashCan))
                {
                    TrashCan ??= appliance;
                    continue;
                }
                if (appliance.GetType() == typeof(SoulImbuer))
                {
                    SoulImbuer ??= appliance;
                    continue;
                }
            }

            ViewingCamera = Object.FindObjectOfType<CameraController>();
            Player = GameManager.PlayerCharacter;

            await PlaySequence(SequenceTime);
        }

        public async Task PlaySequence(float time)
        {
            GameManager.PlayerControls.Disable();
            GameManager.OrderManager.Cancel();
            var originalRotation = ViewingCamera.transform.rotation;
            ViewingCamera.FollowTarget = Player.transform;

            float playerLerpTime = Player.Animator != null
                ? Player.Animator.GetCurrentAnimatorStateInfo(0).length
                : time;
            // Set(Player.transform, -Player.transform.forward, playerLerpTime);
            await Await.Seconds(playerLerpTime);
            ViewingCamera.FollowTarget = null;
            OnStart?.Invoke();

            if (GameManager.CurrentLevelData.DoCountdown)
            {
                Oven.gameObject.SetActive(true);
                OnTargetSet?.Invoke(_CountdownSprites[0]);
                Set(Oven.transform, -Oven.transform.right, 1, _CountdownSprites[0]);
                await Await.Seconds(1);

                TrashCan.gameObject.SetActive(true);
                OnTargetSet?.Invoke(_CountdownSprites[1]);
                Set(TrashCan.transform, -TrashCan.transform.right, 1, _CountdownSprites[1]);
                await Await.Seconds(1);

                SoulImbuer.gameObject.SetActive(true);
                OnTargetSet?.Invoke(_CountdownSprites[2]);
                Set(SoulImbuer.transform, -SoulImbuer.transform.right, 2, _CountdownSprites[2]);
                await Await.Seconds(2);
                
                ViewingCamera.Set(Player.transform.position + ViewingCamera.Offset, originalRotation, time, false, Ease.InOutSine);
                OnGo?.Invoke(SpriteGo);
                await Await.Seconds(time);
            }


            
            foreach (var appliance in GameManager.Appliances)
            {
                if (!appliance.gameObject.activeInHierarchy)
                {
                    appliance.gameObject.SetActive(true);
                }
            }

            ViewingCamera.FollowTarget = Player.transform;
            ViewingCamera.transform.rotation = originalRotation;
            GameManager.PlayerControls.Enable();
            if (GameManager.CurrentLevelData.AutoGenerateOrders) _ = GameManager.OrderManager.Begin();
            else GameManager.OrderManager.BeginLevelManual();

            OnComplete.Set();
            OnEnd?.Invoke();
        }

        public void Set(Transform appliance, Vector3 direction, float duration, Sprite sprite = null)
        {
            _CurrentTarget = appliance;

            if (_CurrentSprite != null)
            {
                _CurrentSprite = sprite;
            }

            float height = Mathf.Max(ViewingCamera.transform.position.y, _CurrentTarget.transform.position.y) -
                           Mathf.Min(ViewingCamera.transform.position.y, _CurrentTarget.transform.position.y);
            float angle = ViewingCamera.transform.eulerAngles.x * Mathf.Deg2Rad;
            float finalAngle = 90 * Mathf.Deg2Rad - angle;
            float length = height * Mathf.Sin(angle) / Mathf.Sin(finalAngle);

            Vector3 targetPosition = _CurrentTarget.transform.position + direction * length / 2 + new Vector3(0, height, 0);

            Vector3 position = _CurrentTarget.transform.position - targetPosition;
            Quaternion rotation = Quaternion.LookRotation(position, ViewingCamera.transform.up);
            Vector3 finalRotation = ViewingCamera.transform.eulerAngles.xnz(rotation.eulerAngles.y);

            ViewingCamera.Set(targetPosition, Quaternion.Euler(finalRotation), duration, false, Ease.InOutSine);
        }
    }
}