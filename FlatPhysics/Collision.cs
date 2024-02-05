using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{

    public static class Collision
    {

        public static bool IntersectCirclePolygon(FlatVector circleCentre, float circleRadius, FlatVector[] vertices, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;

            float axisDepth = 0;
            float minA, maxA, minB, maxB;
            FlatVector axis  = FlatVector.Zero;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector va = vertices[i];
                FlatVector vb = vertices[(i + 1) % vertices.Length];

                FlatVector edge = vb - va;
                axis = new FlatVector(-edge.Y, edge.X);

                Collision.ProjectVertices(vertices, axis, out  minA, out  maxA);
                Collision.ProjectCircle(circleCentre, circleRadius, axis, out  minB, out  maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }


            }

            int cpIndex = Collision.FindClosestPointOnPolygon(circleCentre, vertices);
            FlatVector cp = vertices[cpIndex];

            axis = cp - circleCentre;


            Collision.ProjectVertices(vertices, axis, out  minA, out  maxA);
            Collision.ProjectCircle(circleCentre, circleRadius, axis, out  minB, out  maxB);

            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }

            axisDepth = MathF.Min(maxB - minA, maxA - minB);

            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            depth /= FlatMath.Length(normal);
            normal = FlatMath.Normalize(normal);

            FlatVector polygonCenter = Collision.FindArithmeticMean(vertices);

            FlatVector direction = polygonCenter - circleCentre;

            if (FlatMath.Dot(direction, normal) < 0f)
            {
                normal = -normal;
            }

            return true;
        }

        private static int FindClosestPointOnPolygon(FlatVector circleCentre, FlatVector[] vertices)
        {
            int result = -1;
            float minDistance = float.MaxValue;

            for(int i = 0; i < vertices.Length; i++)
            {
                FlatVector v = vertices[i];
                float distance = FlatMath.Distance(v, circleCentre);

                if(distance < minDistance)
                {
                    minDistance = distance;
                    result = i;

                }
            }
            return result;

        }

        private static void ProjectCircle(FlatVector centre, float radius, FlatVector axis, out float min, out float max)
        {
            FlatVector direction = FlatMath.Normalize(axis);
            FlatVector directionAndRadius = direction * radius;

            FlatVector p1 = centre + directionAndRadius;
            FlatVector p2 = centre - directionAndRadius;

            min = FlatMath.Dot(p1, axis);
            max = FlatMath.Dot(p2, axis);

            if(min > max)
            {
                float t = min;
                min = max;
                max = t;
            }
            
        }

        //Теорема разделяющей оси для квадратов.
        public static bool IntersectPolygons(FlatVector[] verticesA, FlatVector[] verticesB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;

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

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if(axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
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

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            depth /= FlatMath.Length(normal);
            normal = FlatMath.Normalize(normal);

            FlatVector centreA = Collision.FindArithmeticMean(verticesA);
            FlatVector centreB = Collision.FindArithmeticMean(verticesB);

            FlatVector direction = centreB - centreA;

            if(FlatMath.Dot(direction, normal) < 0f)
            {
                normal = -normal;
            }




            return true;
        }

        private static FlatVector FindArithmeticMean(FlatVector[] vertices)
        {
            float sumX = 0f;
            float sumY = 0f;

            for(int i =0; i < vertices.Length; i++)
            {
                FlatVector v = vertices[i];
                sumX += v.X;
                sumY += v.Y;
            }

            return new FlatVector(sumX / (float)vertices.Length, sumY / (float)vertices.Length);
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
