using TDC.Core.Extension;
using UnityEngine;

namespace TDC.Core.Utility.Samplers
{
    public static class Annulus
    {
        private static float _Radians = Mathf.PI * 2;
        public static Vector2[] Points(int count, float innerRadius, float outerRadius, Vector2 origin)
        {
            var points = new Vector2[count];

            float interiorSqrt = Mathf.Sqrt(1 - innerRadius / outerRadius);
            float exteriorPow = Mathf.Pow(innerRadius / outerRadius, 2);
            
            for (var i = 0; i < count; i++)
            {
                float rotation = Random.value * _Radians;
                float distance = Mathf.Sqrt(Random.value * interiorSqrt + exteriorPow) * outerRadius;

                points[i] = Vector2.up.Rotate(rotation) * distance + origin;
            }

            return points;
        }
    }
}