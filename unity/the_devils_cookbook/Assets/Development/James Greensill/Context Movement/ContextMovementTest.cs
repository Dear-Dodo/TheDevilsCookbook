using UnityEngine;

namespace TDC.JamesGreensll
{
    public class ContextMovementTest : MonoBehaviour
    {
        public int Resolution = 8;
        public float Radius;
        public float Range;
        public float Height;

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
            float theta = ((Mathf.PI * 2)) / Resolution;
            for (int i = 0; i < Resolution; i++)
            {
                float angle = (theta * i);
                var start = new Vector3(Radius * Mathf.Cos(angle),0, Radius * Mathf.Sin(angle));
                var end = start + start.normalized * Range;

                start.y = Height;
                end.y = Height;
                Gizmos.DrawLine(transform.position + start, transform.position + end);
            }
        }
    }
}