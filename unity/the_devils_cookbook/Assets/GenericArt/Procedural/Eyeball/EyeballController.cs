using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TDC.Procedural
{
    public class EyeballController : MonoBehaviour
    {
        public float RotationSpeed;
        public GameObject Other;
        public GameObject TargetObject;
        public GameObject DefaultTarget;

        public float ProjectionDistance;
        public float ProjectionRadius;
        public Vector3 Offset;

        private Vector3 _ProjectionOffset;
        private Vector3 _ProjectionLocation;
        private Vector3 _ProjectionDirection;
        private Vector3 _ClampedStartPosition;
        private Vector3 _TargetPosition;
        private Vector3 _RejectedDirection;

        private Vector3 _TargetRotation;

        public Vector3 Target
        {
            get => _TargetPosition;
            set => SetTarget(value);
        }

        public void SetTarget(Vector3 target)
        {
            _TargetPosition = target;
            _ProjectionOffset = -transform.right * ProjectionDistance;
            _ProjectionLocation = transform.position + _ProjectionOffset;
            _ProjectionDirection = transform.position - _ProjectionLocation;

            Vector3 direction = (_TargetPosition - _ProjectionLocation).normalized;
            _RejectedDirection = Vector3.ProjectOnPlane(direction, _ProjectionDirection);
            Vector3 startPosition = _RejectedDirection + _ProjectionLocation;

            Vector3 clampedOffset = _ProjectionLocation - startPosition;
            float distance = clampedOffset.magnitude;

            if (ProjectionRadius < distance)
            {
                Vector3 dir = clampedOffset / distance;
                startPosition = (_ProjectionLocation) - dir * ProjectionRadius;
            }

            _ClampedStartPosition = startPosition;

            Vector3 endPosition = transform.position;
            _TargetRotation = ((endPosition - startPosition)).normalized;
        }

        public void Update()
        {
            if (TargetObject != null)
                SetTarget(TargetObject.transform.position);

            Other.transform.right =
                Vector3.Slerp(Other.transform.right, _TargetRotation, Time.deltaTime * RotationSpeed);
        }

#if UNITY_EDITOR

        public void OnDrawGizmos()
        {
            if (TargetObject != null)
                SetTarget(TargetObject.transform.position);

            Handles.DrawWireDisc(transform.position + _ProjectionOffset, _ProjectionDirection, ProjectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_ProjectionLocation, 0.1f);
            Gizmos.DrawWireSphere(_ClampedStartPosition, 0.1f);
        }

#endif
    }
}