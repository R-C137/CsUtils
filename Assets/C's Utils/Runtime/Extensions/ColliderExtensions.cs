/* ColliderExtensions.cs - C's Utils
 * 
 * Contains various QoL extensions to colliders
 * 
 * 
 * Creation Date: 16/04/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [16/04/2024] - Initial implementation (C137)
 *      
 */
using UnityEngine;

namespace CsUtils.Extensions
{
    public static class ColliderExtensions 
    {
        /// <summary>
        /// Returns a random point within a box collider
        /// </summary>
        /// <param name="boxCollider">The box collider to get the random point from</param>
        /// <returns></returns>
        public static Vector3 GetRandomPoint(this BoxCollider boxCollider)
        {
            return new Vector3(
                Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x),
                Random.Range(boxCollider.bounds.min.y, boxCollider.bounds.max.y),
                Random.Range(boxCollider.bounds.min.z, boxCollider.bounds.max.z));
        }

        /// <summary>
        /// Returns a random point within a sphere collider
        /// <break>
        /// </summary>
        /// <param name="sphereCollider">The sphere collider to get the random point from</param>
        /// <returns></returns>
        public static Vector3 GetRandomPoint(this SphereCollider sphereCollider)
        {
            Vector3 center = sphereCollider.center;
            float radius = sphereCollider.radius;

            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
            Vector3 randomPoint = center + randomDirection * radius;

            return randomPoint;
        }

        /// <summary>
        /// Returns a random point within a mesh collider<br></br>
        /// NOTE: Expensive to compute
        /// </summary>
        /// <param name="meshCollider">The mesh collider to get the random point from</param>
        /// <returns></returns>
        public static Vector3 GetRandomPoint(this MeshCollider meshCollider)
        {
            // Get the mesh from the collider
            Mesh mesh = meshCollider.sharedMesh;

            // Get the triangles of the mesh
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;

            // Choose a random triangle in the mesh
            int randomIndex = Random.Range(0, triangles.Length / 3);

            // Get the vertices of the triangle
            Vector3 vertex1 = vertices[triangles[randomIndex * 3]];
            Vector3 vertex2 = vertices[triangles[randomIndex * 3 + 1]];
            Vector3 vertex3 = vertices[triangles[randomIndex * 3 + 2]];

            // Choose a random point in the triangle
            float barycentricCoord1 = Random.Range(0f, 1f);
            float barycentricCoord2 = Random.Range(0f, 1f);
            if (barycentricCoord1 + barycentricCoord2 > 1)
            {
                barycentricCoord1 = 1 - barycentricCoord1;
                barycentricCoord2 = 1 - barycentricCoord2;
            }
            float barycentricCoord3 = 1 - barycentricCoord1 - barycentricCoord2;

            // Calculate the random point
            Vector3 randomPoint = barycentricCoord1 * vertex1 + barycentricCoord2 * vertex2 + barycentricCoord3 * vertex3;

            return meshCollider.transform.TransformPoint(randomPoint);
        }
    }
}