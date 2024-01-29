using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{

    public static class Collision
    {
        public static bool IntersectPolygons(FlatVector[] verticesA, FlatVector[] verticesB)
        {
            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector va = verticesA[i];
                FlatVector vb = verticesA[(i + 1) % verticesA.Length];

                FlatVector edge = vb - va;
                FlatVector axis = new FlatVector(-edge.Y, edge.X);

                Collision.ProjectVertices(verticesA, axis, out float minA, out float maxA);
                Collision.ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector va = verticesB[i];
                FlatVector vb = verticesB[(i + 1) % verticesB.Length];

                FlatVector edge = vb - va;
                FlatVector axis = new FlatVector(-edge.Y, edge.X);

                Collision.ProjectVertices(verticesA, axis, out float minA, out float maxA);
                Collision.ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ProjectVertices(FlatVector[] vertices, FlatVector axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector v = vertices[i];
                float proj = FlatMath.Dot(v, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }

        //Теорема разделяющей оси.
        public static bool IntersectCircles(FlatVector centreA, float radiusA, FlatVector centreB, float radiusB,
            out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0;

            float distance = FlatMath.Distance(centreA, centreB);
            float radii = radiusA + radiusB;

            if(distance>=radii)
            {
                return false;
            }

            normal = FlatMath.Normalize(centreB - centreA);
            depth = radii - distance;

            return true;
        }
    }
}
