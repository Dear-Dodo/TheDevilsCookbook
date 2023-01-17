using UnityEngine;

namespace TDC.AIRefac.Behaviour
{
    public static class ObstacleAvoidance
    {
        public static Vector3 AvoidCast(Vector3 origin, Vector3 heading, float angle, int castCount, float distance, out float factor)
        {
            Debug.Assert(castCount > 1);
            float anglePerCast = angle / castCount - 1;

            Vector3 startDirection = Quaternion.Euler(0, -angle / 2.0f, 0) * heading;
            var hitCount = 0;
            Vector3 adjustedHeading = Vector3.zero;
            factor = 0;
            for (var i = 0; i < castCount; i++)
            {
                float offset = (i) * anglePerCast;
                Vector3 castDirection = Quaternion.Euler(0, offset, 0) * startDirection;
                if (!Physics.Raycast(origin, castDirection, out RaycastHit hit, distance, ~LayerMask.GetMask("Castable Surface")))
                {
                    Debug.DrawRay(origin, castDirection * distance, Color.green);
                    continue;
                };
                float distanceFactor = hit.distance / distance;
                Vector3 avoidDirection = -castDirection;
                // Vector3 avoidDirection = -Vector3.Reflect(castDirection, heading);
                adjustedHeading += avoidDirection * distanceFactor;
                factor = Mathf.Max(distanceFactor, factor);
                Debug.DrawRay(origin, castDirection * hit.distance, Color.red);
                hitCount++;
            }

            return hitCount > 0 ? (adjustedHeading / hitCount).normalized : heading;
        }
    }
}