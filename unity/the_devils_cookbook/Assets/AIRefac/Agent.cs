using TDC.Affectables;
using TDC.Core.Manager;
using TDC.Core.Utility;
using UnityEngine;

namespace TDC.AIRefac
{
    [RequireComponent(typeof(AffectableStats), typeof(Rigidbody), typeof(MomentumController))]
    [RequireComponent(typeof(ColliderProxy))]
    public class Agent : MonoBehaviour
    {
        protected StateMachine StateMachine;
        
        protected AffectableStats _Stats;
        public AffectableStats Stats => _Stats;
        public Rigidbody Rigidbody { get; private set; }

        public Collider Collider { get; private set; }
        
        public ColliderProxy ColliderProxy { get; private set; }

        public MomentumController MomentumController { get; private set; }

        public Animator Animator { get; private set; }

        public Transform Transform { get; private set; }

        public int NavmeshAgentID;
        
        protected virtual void Awake()
        {
            _Stats = GetComponent<AffectableStats>();
            Rigidbody = GetComponent<Rigidbody>();
            MomentumController = GetComponent<MomentumController>();
            Animator = GetComponentInChildren<Animator>();
            Transform = transform;
            Collider = GetComponent<Collider>();
            ColliderProxy = GetComponent<ColliderProxy>();
        }

        public virtual void Enable()
        {
            GameManager.RunOnInitialisation(() => _ = StateMachine.Run());
        }
        
        protected virtual void OnDisable()
        {
            StateMachine.Stop();
        }

        protected virtual void OnDestroy()
        {
            StateMachine.Stop();
        }

        private void OnDrawGizmosSelected()
        {
            StateMachine?.OnDrawGizmosSelected();
        }

        public void AddForce(Vector3 force)
        {
            GetComponent<Rigidbody>().velocity += force;
        }
    }
}