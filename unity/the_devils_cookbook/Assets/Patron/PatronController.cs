using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TDC.Patrons
{
    public class PatronController : MonoBehaviour
    {
        public float Speed;
        public float PointDistance;

        public Vector3 EffectorTarget;
        public Vector3 TipTarget;

        public GameObject Target;
        public Vector3 Normal;

        public List<GameObject> Bones;

        public AnimationCurve XAxisCurve;
        public AnimationCurve YAxisCurve;
        public AnimationCurve ZAxisCurve;

        public NavMeshAgent Agent;

        public void Start()
        {
        }

        public void Update()
        {
            //EffectorTarget = Target.transform.position + Normal * Speed;
            //Move();
        }

        private void Move()
        {
            for (int i = 0; i < Bones.Count - 1; i++)
            {
                var diff = Bones[i + 1].transform.position - Bones[i].transform.position;

                if (diff.magnitude > PointDistance)
                {
                    Bones[i].transform.position += diff.normalized * Speed;
                }
                //Solve();
            }
        }

        private void Solve()
        {
            float[] lengths = new float[Bones.Count - 1];
            for (int i = 0; i < Bones.Count - 1; i++)
            {
                var diff = Bones[i + 1].transform.position - Bones[i].transform.position;
                lengths[i] = diff.magnitude;
            }
            for (int i = 0; i < lengths.Length - 1; i++)
            {
                var pivotDirection = Bones[i + 1].transform.position - Bones[i].transform.position;
                Bones[i].transform.rotation = Quaternion.LookRotation(pivotDirection);

                var uTt = EffectorTarget - Bones[i].transform.position;

                var a = lengths[i];
                var b = lengths[i + 1];
                var c = uTt.magnitude;

                var B = Mathf.Acos((c * c + a * a - b * b) / (2 * c * a)) * Mathf.Rad2Deg;
                var C = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) * Mathf.Rad2Deg;

                if (!float.IsNaN(C))
                {
                    var upperRotation = Quaternion.AngleAxis((-B), Vector3.right);
                    Bones[i].transform.localRotation = upperRotation;
                    var lowerRotation = Quaternion.AngleAxis(180 - C, Vector3.right);
                    Bones[i + 1].transform.localRotation = lowerRotation;
                }
            }
        }

        public void OnDrawGizmos()
        {
        }
    }
}