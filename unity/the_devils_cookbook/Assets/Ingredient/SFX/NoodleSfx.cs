using FMODUnity;
using UnityEngine;

namespace TDC
{
    public class NoodleSfx : MonoBehaviour
    {
        public StudioEventEmitter EventEmitter;
        public Rigidbody Rigidbody;

        void Update()
        {
            EventEmitter.SetParameter("Speed", Rigidbody.velocity.magnitude);
        }
    }
}
