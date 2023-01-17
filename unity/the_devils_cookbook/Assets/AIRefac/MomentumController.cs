using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TDC.AIRefac
{
    public class MomentumController : MonoBehaviour
    {
        private Vector3 _CurrentVelocity;
        
        private Vector3 _DragNormal = Vector3.forward;
        private float _DragDelta;

        private Vector3 _VelocityTarget;
        private float _AccelerationDelta;

        private LinkedList<Vector3> _Influences = new LinkedList<Vector3>();
        private Vector3 _ApplyOnce;
        private Vector3 _NextVelocitySet;
        private bool _SetNextVelocity;

        private Rigidbody _Rigidbody;
        
        public void SetDrag(Vector3 normal, float delta)
        {
            _DragNormal = normal;
            _DragDelta = delta;
        }

        public void SetTarget(Vector3 target, float delta)
        {
            _VelocityTarget = target;
            _AccelerationDelta = delta;
        }

        public Vector3 GetTarget()
        {
            return _VelocityTarget;
        }

        public void AddForce(Vector3 velocity)
        {
            _CurrentVelocity += velocity;
        }

        public void ChangeSpeedOnce(Vector3 delta)
        {
            _ApplyOnce += delta;
        }
        public LinkedListNode<Vector3> AddInfluence(Vector3 velocity)
        {
            return _Influences.AddLast(velocity);
        }

        public void RemoveInfluence(LinkedListNode<Vector3> node)
        {
            _Influences.Remove(node);
        }
        
        public void RemoveInfluence(Vector3 velocity)
        {
            _Influences.Remove(velocity);
        }

        public void SetVelocity(Vector3 velocity)
        {
            _NextVelocitySet = velocity;
            _SetNextVelocity = true;
        }

        public void SetVelocityAndTarget(Vector3 velocity)
        {
            SetVelocity(velocity);
            SetTarget(velocity, _AccelerationDelta);
        }
        
        private void Awake()
        {
            _Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (_SetNextVelocity)
            {
                _CurrentVelocity = _NextVelocitySet;
                _SetNextVelocity = false;
            }
            _CurrentVelocity += _ApplyOnce;
            _ApplyOnce = Vector3.zero;
            foreach (Vector3 influence in _Influences)
            {
                _CurrentVelocity += influence * Time.fixedDeltaTime;
            }
            _CurrentVelocity = Vector3.MoveTowards(_CurrentVelocity, 
                Vector3.Project(_CurrentVelocity, _DragNormal), _DragDelta);
            _CurrentVelocity = Vector3.MoveTowards(_CurrentVelocity, _VelocityTarget, _AccelerationDelta);
            _Rigidbody.velocity = _CurrentVelocity;
        }
    }
}
