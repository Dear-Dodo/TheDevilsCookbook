using TDC.Affectables;
using TDC.AIRefac.States;
using UnityEngine;

namespace TDC.AIRefac.Ingredients
{
    public class NoodleAgent : Agent
    {
        [SerializeField] private TrailRenderer _TrailRenderer;
        [SerializeField] private float _Thickness;
        [SerializeField] private Effect _StatusEffect;

        private Wander _WanderState = new Wander();

        protected override void Awake()
        {
            base.Awake();
            StateMachine = new StateMachine(this);

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

            StateMachine.AddState(_WanderState);
            StateMachine.AddState(StunnedState);
        }

        public override void Enable()
        {
            base.Enable();
            _TrailRenderer.enabled = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _TrailRenderer.enabled = false;
        }

        protected void Update()
        {
            if (Stats.ModifiedStats["Silenced"] > 0)
            {
                _TrailRenderer.emitting = false;
            }
            else if (enabled)
            {
                _TrailRenderer.emitting = true;
                for (int i = 0; i < _TrailRenderer.positionCount - 1; i++)
                {
                    Ray ray = new Ray
                    {
                        origin = _TrailRenderer.GetPosition(i),
                        direction = _TrailRenderer.GetPosition(i + 1) - _TrailRenderer.GetPosition(i)
                    };
                    RaycastHit[] raycastHits = Physics.SphereCastAll(ray, _Thickness, Vector3.Distance(_TrailRenderer.GetPosition(i + 1), _TrailRenderer.GetPosition(i)));
                    foreach (RaycastHit hit in raycastHits)
                    {
                        if (hit.collider.TryGetComponent(out AffectableStats targetStats) && !hit.collider.TryGetComponent<NoodleAgent>(out _))
                        {
                            targetStats.AddEffect(_StatusEffect);
                        }
                    }
                }
            }
        }
    }
}