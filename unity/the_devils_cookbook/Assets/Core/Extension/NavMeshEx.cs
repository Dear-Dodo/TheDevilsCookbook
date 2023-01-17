using System;
using System.Collections.Generic;
using TDC.Core.Type;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace TDC.Core.Extension
{
    public static class NavMeshEx
    {
        public static Vector3 SampleRandomPoint(out List<Tuple<float, Triangle>> weightedTriangles)
        {
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            weightedTriangles = new List<Tuple<float, Triangle>>();

            float totalWeight = 0.0f;

            for (int i = 0; i < triangulation.indices.Length / 3; i++)
            {
                Triangle triangle = new Triangle();
                for (int j = 0; j < 3; j++)
                {
                    triangle.Vertices[j] = triangulation.vertices[triangulation.indices[i * 3 + j]];
                }

                float weight = triangle.GetArea();
                totalWeight += weight;
                weightedTriangles.Add(new Tuple<float, Triangle>(weight, triangle));
            }

            weightedTriangles.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            float randomWeight = Random.Range(0, totalWeight);

            float currentArea = 0.0f;
            Triangle selectedTriangle = weightedTriangles[Random.Range(0, weightedTriangles.Count)].Item2;

            foreach ((float weight, Triangle triangle) in weightedTriangles)
            {
                if (currentArea < randomWeight)
                {
                    currentArea += weight;
                    continue;
                }

                selectedTriangle = triangle;
                break;
            }

            return selectedTriangle.RandomPoint();
        }

        public static Vector3 CalculatePoint(Vector3 point, float maxDistance, int areaMask)
        {
            NavMesh.SamplePosition(point, out var hit, maxDistance, areaMask);
            return hit.position;
        }
    }
}