using TDC.Player;
using UnityEngine;

namespace TDC.Affectables
{
    public abstract class Projectile : MonoBehaviour
    {
        [SerializeField]
        private Effect _Effect;
        public float Speed;
        public float Range;
        public LayerMask LayerMask;
        public bool Deflected = false;

        protected Path Path;
        protected Vector3 Target;
        private int _Index;

        public abstract Path Initialize(Vector3 start, Vector3 end);

        public void Update()
        {
            if (Path == null)
            {
                return;
            }

            

            Transform projectileTransform = transform;
            Vector3 position = projectileTransform.position;
            float distance = Speed * Time.deltaTime;
            while (distance > 0)
            {
                distance -= Vector3.Distance(position, Target);
                position = Vector3.MoveTowards(position, Target, Speed * Time.deltaTime);
                ApplyEffect();
                if (Vector3.Distance(position, Target) < 0.01f)
                {
                    if (_Index >= Path.Points.Length - 1)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    Target = Path.Points[_Index + 1];
                    _Index++;
                }
            }
            
            projectileTransform.position = position;
        }

        private void ApplyEffect()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Range, LayerMask, QueryTriggerInteraction.Collide);
            foreach (Collider coll in colliders)
            {
                var body = coll.GetComponentInChildren<AffectableStats>();
                if (body != null && (Deflected || coll.TryGetComponent<PlayerCharacter>(out _)))
                {
                    body.AddEffect(_Effect);
                    Destroy(gameObject);
                }
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, Range);
        }
    }
}