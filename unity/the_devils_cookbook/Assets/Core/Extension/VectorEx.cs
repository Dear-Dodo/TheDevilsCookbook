using UnityEngine;

namespace TDC.Core.Extension
{
    public static class VectorEx
    {
        public static Vector2 Rotate(this Vector2 v, float radians)
        {
            return new Vector2()
            {
                x = Mathf.Cos(radians) * v.x - Mathf.Sin(radians) * v.y,
                y = Mathf.Sin(radians) * v.x - Mathf.Cos(radians) * v.y
            };
        }
    }
}