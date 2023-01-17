using UnityEngine;
using Utility;

namespace TDC.Core.Utility
{
    public static class SectorMeshGenerator
    {
        private static void BuildTri(ref int[] indices, int index, int vert1, int vert2, int vert3)
        {
            indices[index] = vert1;
            indices[index + 1] = vert2;
            indices[index + 2] = vert3;
        }
        
        private static void BuildMeshLayer(ref Vector3[] verts, ref Vector2[] uvs, ref int[] indices,
            int vertStart, int vertEnd, int triStart, int triEnd, 
            Vector3 offset, Vector3 leftPoint, Vector3 rightPoint, float factorPerIteration, float radius)
        {
            verts[vertStart] = offset;
            float t = 0;
            for (int i = vertStart + 1; i < vertEnd; i++)
            {
                verts[i] = Vector3.Slerp(leftPoint, rightPoint, t) + offset;
                uvs[i] = Vector3.Slerp(Quaternion.AngleAxis(-45, Vector3.up) * Vector3.forward, Quaternion.AngleAxis(45,Vector3.up) * Vector3.forward, t).xz();
                t += factorPerIteration;
            }
            // Build tris
            int vertOffset = vertStart;
            for (int i = triStart; i < triEnd; i++)
            {
                int firstIndex = i * 3;
                BuildTri(ref indices, firstIndex, vertStart, vertOffset+1, vertOffset+2);
                vertOffset++;
            }
        }

        public static Mesh BuildSquareUV2D(float radius, float angle, float resolution)
        {
            int secondaryVertices = Mathf.Max(2, Mathf.RoundToInt(angle * resolution));
            int vertexCount = secondaryVertices * 2;
            var sectorMesh = new Mesh();
            
            Vector3 rightPoint = Quaternion.AngleAxis(angle/2.0f, Vector3.up) 
                                 * new Vector3(0, 0, radius);
            sectorMesh.bounds = new Bounds(new Vector3(0, 0, radius / 2.0f),
                new Vector3(rightPoint.x * 2, 0, radius));
            var leftPoint = new Vector3(-rightPoint.x, 0, rightPoint.z);
            
            float factorPerIteration = 1.0f / (secondaryVertices - 1.0f);
            float t = 0;
            
            var vertices = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];

            int triCount = (secondaryVertices - 1) * 2;
            var indices = new int[triCount * 3];
            for (var i = 0; i < vertexCount; i += 2)
            {
                vertices[i + 1] = Vector3.Slerp(leftPoint, rightPoint, t);
                vertices[i] = vertices[i + 1].normalized * 0.01f;
                uvs[i + 1] = new Vector2(t, 1);
                uvs[i] = new Vector2(t, 0);
                t += factorPerIteration;
            }

            for (var i = 0; i < triCount; i += 2)
            {
                int firstIndex = i * 3;
                BuildTri(ref indices, firstIndex, i+1, i+3, i);
                BuildTri(ref indices, firstIndex+3, i+3, i+2, i);
            }
            
            sectorMesh.vertices = vertices;
            sectorMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            sectorMesh.SetUVs(0, uvs);
            sectorMesh.RecalculateNormals();
            return sectorMesh;
        }
        
        
        public static Mesh Build3D(float radius, float angle)
        {
            const float halfHeight = 0.2f;
            var heightOffset = new Vector3(0, halfHeight, 0);
            // vertices per degree
            const float resolution = 0.25f;
            var coneMesh = new Mesh();
            // Circumferencial vertices
            int secondaryVerticesPerLayer = Mathf.Max(2, Mathf.RoundToInt(angle * resolution));
            int verticesPerLayer = secondaryVerticesPerLayer + 1;
            int secondLayerStart = verticesPerLayer;

            Vector3 rightPoint = Quaternion.AngleAxis(angle/2.0f, Vector3.up) 
                                 * new Vector3(0, 0, radius);
            coneMesh.bounds = new Bounds(new Vector3(0, 0, radius / 2.0f),
                new Vector3(rightPoint.x * 2, halfHeight * 2, radius));
            var leftPoint = new Vector3(-rightPoint.x, 0, rightPoint.z);

            float factorPerIteration = 1.0f / (secondaryVerticesPerLayer - 1.0f);
            float t = 0;
            
            var vertices = new Vector3[verticesPerLayer * 2];
            var uvs = new Vector2[verticesPerLayer * 2];
            
            vertices[0] = -heightOffset;
            uvs[0] = new Vector2(0.5f, 0);

            int trisPerLayer = secondaryVerticesPerLayer - 1;
            int totalLayerTris = trisPerLayer * 2;
            int arcTriCount = trisPerLayer * 2;
            const int sideTriCount = 2 * 2;
            var indices = new int[(totalLayerTris + arcTriCount + sideTriCount) * 3];
            
            BuildMeshLayer(ref vertices, ref uvs, ref indices, 0, secondaryVerticesPerLayer + 1,
                0, trisPerLayer, -heightOffset, leftPoint, rightPoint, factorPerIteration, radius);
            BuildMeshLayer(ref vertices, ref uvs, ref indices, secondLayerStart, vertices.Length,
                trisPerLayer, totalLayerTris, heightOffset, leftPoint, rightPoint, factorPerIteration, radius);

            // Build arc tris
            var triOffset = 1;
            for (int i = totalLayerTris; i < totalLayerTris + arcTriCount; i += 2)
            {
                int firstTriIndex = i * 3;
                BuildTri(ref indices, firstTriIndex, triOffset,
                    verticesPerLayer + triOffset, verticesPerLayer + triOffset + 1);

                int secondTriIndex = firstTriIndex + 3;
                BuildTri(ref indices, secondTriIndex, 
                    triOffset, verticesPerLayer + triOffset + 1, triOffset + 1);

                triOffset++;
            }
            
            // Side tris
            int firstSideIndex = (totalLayerTris + arcTriCount) * 3;
            BuildTri(ref indices, firstSideIndex, secondLayerStart, 0, 1 );
            BuildTri(ref indices, firstSideIndex+3, secondLayerStart, 1, secondLayerStart + 1);
            int secondSideIndex = firstSideIndex + 6;
            BuildTri(ref indices, secondSideIndex, secondLayerStart, secondLayerStart - 1, 0);
            BuildTri(ref indices, secondSideIndex+3, secondLayerStart, vertices.Length - 1, secondLayerStart - 1);
            
            coneMesh.vertices = vertices;
            coneMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            coneMesh.SetUVs(0, uvs);
            coneMesh.RecalculateNormals();
            return coneMesh;
        }
    }
}