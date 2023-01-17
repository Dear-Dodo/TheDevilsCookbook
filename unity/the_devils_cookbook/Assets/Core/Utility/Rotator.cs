using UnityEngine;

namespace TDC.Core.Utility
{
    public class Rotator : MonoBehaviour
    {
        public bool Rotate;
        public float Angle;
        public Vector3 Axes;

        public void Update()
        {
            if (Rotate)
            {
                transform.Rotate(Axes, Angle);
            }
        }
    }
}
