using System.Collections.Generic;
using UnityEngine;

namespace TDC.Core.Utility
{
    public static class Bezier
    {
        // precalculated

        private static float[] _Factorial = new float[]
        {
            1.0f,
            1.0f,
            2.0f,
            6.0f,
            24.0f,
            120.0f,
            720.0f,
            5040.0f,
            40320.0f,
            362880.0f,
            3628800.0f,
            39916800.0f,
            479001600.0f,
            6227020800.0f,
            87178291200.0f,
            1307674368000.0f,
            20922789888000.0f,
        };

        public static Vector3[] CalculatePoints(List<Vector3> controlPoints, float resolution)
        {
            var tick = 1.0f / resolution;
            List<Vector3> points = new List<Vector3>();
            for (var i = 0; i < resolution; i++)
            {
                Vector3 point = CalculatePoint(controlPoints, tick * i);
                points.Add(point);
            }

            return points.ToArray();
        }

        public static Vector3 CalculatePoint(List<Vector3> controlPoints, float t) =>
            _CalculatePoint(new List<Vector3>(controlPoints), t);

        private static Vector3 _CalculatePoint(List<Vector3> controlPoints, float t)
        {
            if (controlPoints.Count == 2)
            {
                return Vector3.Lerp(controlPoints[0], controlPoints[1], t);
            }

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                controlPoints[i] = Vector3.Lerp(controlPoints[i], controlPoints[i + 1], t);
            }
            controlPoints.RemoveAt(controlPoints.Count - 1);
            return _CalculatePoint(controlPoints, t);
        }

        public static Vector3 CalculateAutoTangent(Vector3 start, Vector3 end, Vector3 axes, float dropoff, float intensity) => Vector3.Lerp(start, end, dropoff) + (axes * (intensity + 1));
    }

#if UNITY_EDITOR

    /// <summary>
    /// Anything from this class is to be used within the editor only.
    /// </summary>
    public static class BezierEditor
    {
        public static void DrawBezier(Transform transform, List<Vector3> controlPoints, float resolution, Color color)
        {
            Vector3[] points = Bezier.CalculatePoints(controlPoints, resolution);
            Gizmos.color = color;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(transform.position + points[i], transform.position + points[i + 1]);
            }

            Gizmos.color = Color.white;
        }
    }

#endif
}