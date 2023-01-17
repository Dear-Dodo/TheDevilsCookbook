using System;
using UnityEngine;

namespace TDC.Core.Utility
{
    public class ColliderProxy : MonoBehaviour
    {
        public event Action<Collider> TriggerEntered;
        public event Action<Collider> TriggerExited;
        public event Action<Collider> TriggerStayed;

        public event Action<Collision> CollisionEntered;
        public event Action<Collision> CollisionExited;
        public event Action<Collision> CollisionStayed;
        
        private void OnTriggerEnter(Collider other) => TriggerEntered?.Invoke(other);
        private void OnTriggerExit(Collider other) => TriggerExited?.Invoke(other);
        private void OnTriggerStay(Collider other) => TriggerStayed?.Invoke(other);

        private void OnCollisionEnter(Collision collision) => CollisionEntered?.Invoke(collision);
        private void OnCollisionExit(Collision collision) => CollisionExited?.Invoke(collision);
        private void OnCollisionStay(Collision collision) => CollisionStayed?.Invoke(collision);
    }
}