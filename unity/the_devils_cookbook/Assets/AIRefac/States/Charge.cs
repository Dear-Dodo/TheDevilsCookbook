using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDC.Affectables;
using TDC.AIRefac.Actions;
using TDC.Core.Manager;
using UnityAsync;
using UnityEngine;

namespace TDC.AIRefac.States
{
    public class Charge : State
    {
        public float WindupTime = 2.0f;
        public float RecoveryTime = 2.0f;
        public float ChargeSpeedFactor = 2.0f;
        public float ChargeRotationFactor = 0.2f;
        private const float Acceleration = 40.0f;
        public Effect PlayerEffect;
        private static readonly int _StartChargeUp = Animator.StringToHash("StartChargeUp");
        private static readonly int _StartCharge = Animator.StringToHash("StartCharge");
        private static readonly int _StartRecover = Animator.StringToHash("StartRecover");
        private static readonly int _StartIdle = Animator.StringToHash("StartIdle");

        protected override Task OnStateStart(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async Task RunBehaviour(StateMachine runner, CancellationToken token)
        {
            Transform playerTransform = GameManager.PlayerCharacter.transform;
            Transform agentTransform = runner.AttachedAgent.Transform;
            
            // Turn to player first
            var rotateTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var rotateAction = new RotateToLookat(runner, playerTransform);
            _ = rotateAction.Run(rotateTokenSource.Token);

            await Await.Until(() =>
            {
                Vector3 toTarget = playerTransform.position - agentTransform.position;
                return Mathf.Abs(Vector3.SignedAngle(agentTransform.forward, toTarget, Vector3.up)) < 15;
            });
            
            // Charge up
            runner.AttachedAgent.Animator.SetTrigger(_StartChargeUp);
            rotateAction.RotationSpeedModifier = ChargeRotationFactor;
            await Await.Seconds(WindupTime).ConfigureAwait(token);
            
            // Can see player?
            var ray = new Ray(runner.AttachedAgent.Collider.bounds.center, playerTransform.position - runner.AttachedAgent.Transform.position);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.LogWarning("Raycast did not hit.");
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 5.0f);
                rotateTokenSource.Cancel();
                return;
            }

            if (hit.collider.gameObject != GameManager.PlayerCharacter.gameObject)
            {
                Debug.Log($"Raycast hit {hit.collider.gameObject}.");
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 5.0f);
                rotateTokenSource.Cancel();
                return;
            }
            
            // Charge
            runner.AttachedAgent.Animator.SetTrigger(_StartCharge);

            var isCharging = true;

            var shouldRecover = false;

            List<ContactPoint> contacts = new List<ContactPoint>();
            
            void OnCollision(Collision collision)
            {
                // Castable surface
                if (collision.gameObject.layer == 6 || !isCharging) return;
                collision.GetContacts(contacts);
                if (contacts.All(c => Vector3.Dot(c.normal, Vector3.up) > 0.8f)) return;
                if (collision.gameObject == GameManager.PlayerCharacter.gameObject)
                {
                    Debug.Log($"STUN! {collision.gameObject.name}");
                    GameManager.PlayerCharacter.GetComponent<AffectableStats>().AddEffect(PlayerEffect);
                    isCharging = false;
                    return;
                }

                isCharging = false;
                runner.AttachedAgent.Animator.SetTrigger(_StartRecover);
                shouldRecover = true;
                Debug.Log($"RECOVER! {collision.gameObject.name}");
            }

            runner.AttachedAgent.ColliderProxy.CollisionEntered += OnCollision;
            
            while (isCharging && !token.IsCancellationRequested)
            {
                Vector3 forward = runner.AttachedAgent.Transform.forward;
                float moveSpeed = runner.AttachedAgent.Stats.ModifiedStats["Movement Speed"] * ChargeSpeedFactor;
                                  runner.AttachedAgent.MomentumController.SetDrag(forward, 5.0f);
                runner.AttachedAgent.MomentumController.SetTarget(forward * moveSpeed, Acceleration * Time.fixedDeltaTime);
                await new WaitForFixedUpdate();
                await Await.NextFixedUpdate();
            }
            
            runner.AttachedAgent.ColliderProxy.CollisionEntered -= OnCollision;
            runner.AttachedAgent.MomentumController.SetVelocityAndTarget(Vector3.zero);
            rotateTokenSource.Cancel();
            if (shouldRecover) await Await.Seconds(RecoveryTime).ConfigureAwait(token);
            runner.AttachedAgent.Animator.SetTrigger(_StartIdle);
        }

        protected override Task OnStateEnd(StateMachine runner, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}