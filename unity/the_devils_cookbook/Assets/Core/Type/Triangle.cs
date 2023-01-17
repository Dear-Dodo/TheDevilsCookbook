using UnityEngine;

namespace TDC.Core.Type
{
    public class Triangle
    {
        public Vector3[] Vertices;
        
        public Triangle()
        {
            Vertices = new Vector3[3];
        }

        public float GetArea()
        {
            Vector3 p1 = Vertices[0];
            Vector3 p2 = Vertices[1];
            Vector3 p3 = Vertices[2];

            Vector3 cross = Vector3.Cross(p1 - p2, p1 - p3);
            return cross.magnitude * 0.5f;
        }

        public Vector3 RandomPoint()
        {
            float r1 = Random.Range(0.0f, 1.0f);
            float r2 = Random.Range(0.0f, 1.0f);

            return (1 - Mathf.Sqrt(r1)) * Vertices[0] +
                   (Mathf.Sqrt(r1) * (1 - r2) * Vertices[1] + (Mathf.Sqrt(r1) * r2) * Vertices[2]);
        }
    }
}