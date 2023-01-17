using DG.Tweening;
using UnityEngine;

namespace TDC.Player
{
    public class CameraController : MonoBehaviour
    {
        public float CameraSpeed;
        public float RotationSpeed;

        public bool AutoCalculateSpeed;

        public float MinTime;

        public Vector3 Offset;

        public Transform FollowTarget;

        public Quaternion RotationTarget;
        public Vector3 PositionTarget;
        

        private float _StepSize;

        public void Set(Vector3 pTarget, Quaternion rTarget, float duration, bool durationIsSpeed, DG.Tweening.Ease ease = Ease.Linear)
        {
            PositionTarget = pTarget;
            RotationTarget = rTarget;

            if (durationIsSpeed)
            {
                duration = Vector3.Distance(transform.position, PositionTarget) / duration;
            }

            transform.DOMove(pTarget, duration).SetEase(ease);
            transform.DORotateQuaternion(rTarget, duration).SetEase(ease);
        }

        public void LateUpdate()
        {
            if (FollowTarget)
            {
                transform.position = FollowTarget.transform.position + Offset;
            }
        }
    }
}