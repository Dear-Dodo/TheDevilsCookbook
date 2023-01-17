using UnityEngine;

namespace TDC.JamesGreensll
{
    public class T_AutoPositionTriangle : MonoBehaviour
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public float height;
        public float angle;

        public void OnDrawGizmos()
        {
            A.y = 0;
            A.y = 0;

            B.y = height;
            B.x = 0;

            float angleB = angle;
            float angleA = 90;
            float angleC = 180 - angleA - angleB;

            var sinLaw = height / Mathf.Sin(angleC);
            var b = (sinLaw * Mathf.Sin(angle)) / Mathf.Sin(angleA);

            C.x = b;
            C.y = 0;

            Gizmos.DrawLine(A, B);
            Gizmos.DrawLine(B, C);
            Gizmos.DrawLine(C, A);
        }
    }
}