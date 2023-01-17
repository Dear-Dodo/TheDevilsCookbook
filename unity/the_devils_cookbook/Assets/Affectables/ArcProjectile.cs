using System.Collections.Generic;
using TDC.Core.Utility;
using UnityEngine;

namespace TDC.Affectables
{
    public class ArcProjectile : Projectile
    {
        public float Step = 10;
        public float Overshoot = 2;
        public GameObject hitFX;
        public override Path Initialize(Vector3 start, Vector3 end)
        {
            

            List<Vector3> points = new List<Vector3> { start };

            Vector3[] pointsArrray =
                Bezier.CalculatePoints(
                    new List<Vector3> { start, Bezier.CalculateAutoTangent(start, start + (Overshoot * (end - start)), Vector3.up, 0.5f, 1.0f), end },
                    Step);

            points.AddRange(pointsArrray);

            points.Add(end);

            Path = new Path(points.ToArray());

            Target = Path.Points[0];

            return Path;
        }

        private void OnDestroy()
        {
            Destroy(Instantiate(hitFX, transform.position, Quaternion.identity), 1.0f);
        }
    }
}