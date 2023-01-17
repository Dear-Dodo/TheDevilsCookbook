using UnityEngine;

namespace TDC.Core.Utility
{
    public class Billboard : MonoBehaviour
    {
        public Camera Camera;

        public void Awake()
        {
            if (Camera == null)
            {
                Camera = Camera.main;
            }
        }

        public void LateUpdate()
        {
            transform.rotation = Camera.transform.rotation;
        }
    }
}