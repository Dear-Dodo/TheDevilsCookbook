using Newtonsoft.Json.Schema;
using TDC.Affectables;
using TDC.AIRefac.States;
using TDC.Core.Manager;
using UnityEngine;
using UnityEngine.AI;

namespace TDC.AIRefac.Ingredients
{
    public class MeatAgent : Agent
    {
        [Header("Obstacle Avoidance")]
        public float CastAngle = 75.0f;
        public int CastCount = 8;
        public float CastDistance = 2.0f;

        private Wander _WanderState = new Wander();

        [Header("Charge")]
        public float ChargeCooldown = 8.0f;
        public float MaxStartRange = 10.0f;
        public float MinStartRange = 2.0f;
        public Effect ChargeEffect;
        private float _LastCharge = 0;
        
        private void OnValidate()
        {
            _WanderState.Angle = CastAngle;
            _WanderState.CastCount = CastCount;
            _WanderState.CastDistance = CastDistance;
        }

        protected override void Awake()
        {
            base.Awake();
            StateMachine = new StateMachine(this);
            StateMachine.AddState(_WanderState);
            NavmeshAgentID = NavMesh.GetSettingsByIndex(1).agentTypeID;

            bool WanderTransitionCondition()
            {
                Vector3 playerPosition = GameManager.PlayerCharacter.transform.position;
                if (Stats.ModifiedStats["Silenced"] > 0) return false;
                
                float playerDistance = Vector3.Distance(transform.position, playerPosition);
                bool isInRange = MinStartRange < playerDistance && playerDistance < MaxStartRange;
                bool isChargeReady = Time.time - _LastCharge >= ChargeCooldown;
                if (!isChargeReady || !isInRange) return false;
                
                var ray = new Ray(Collider.bounds.center, playerPosition - Transform.position);
                bool canSeePlayer = Physics.Raycast(ray, out RaycastHit hit) &&
                                    hit.collider.gameObject == GameManager.PlayerCharacter.gameObject;
                
                if (canSeePlayer) _LastCharge = Time.time;
                return canSeePlayer;
            }

            bool ChargeCancelSilencedCondition()
            {
                return Stats.ModifiedStats["Silenced"] > 0;
            }

            var charge = new Charge()
            {
                PlayerEffect = ChargeEffect
            };
            _WanderState.AddTransition(new Transition(charge, false, WanderTransitionCondition));
            charge.AddTransition(new Transition(_WanderState, true));
            charge.AddTransition(new Transition(_WanderState, false, ChargeCancelSilencedCondition));

            bool IsStunned()
            {
                return Stats.ModifiedStats["Stunned"] > 0;
            }

            bool IsNotStunned()
            {
                return !IsStunned();
            }

            var StunnedState = new Stunned();
            StateMachine.AddGlobalTransition(new Transition(StunnedState, false, IsStunned));
            StunnedState.AddTransition(new Transition(_WanderState, false, IsNotStunned));

            StateMachine.AddState(charge);
            StateMachine.AddState(StunnedState);
        }


    }
}