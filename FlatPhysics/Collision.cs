using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{

    public static class Collision
    {

        public static void FindContactPoints(
            FlatBody bodyA, FlatBody bodyB,
            out FlatVector contact1, out FlatVector contact2,
            out int contactCount)
        {
            contact1 = FlatVector.Zero;
            contact2 = FlatVector.Zero;
            contactCount = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {

                }
                else if (shapeTypeB is ShapeType.Circle)
                {

                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {

                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    Collision.FindContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, out contact1);
                    contactCount = 1;
                }
            }
        }

        private static  void FindContactPoint(FlatVector centerA, float radiusA, FlatVector centerB, out FlatVector cp)
        {
            FlatVector ab = centerB -  centerA;
            FlatVector dir = FlatMath.Normalize(ab);
            cp = centerA + dir * radiusA;

            
        }


        public static bool Collide(FlatBody bodyA, FlatBody bodyB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collision.IntersectPolygons(
                        bodyA.Position, bodyA.GetTransformedVertice(),
                        bodyB.Position, bodyB.GetTransformedVertice(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    bool result = Collision.IntersectCirclePolygon(
                        bodyB.Position, bodyB.Radius,
                        bodyA.Position, bodyA.GetTransformedVertice(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collision.IntersectCirclePolygon(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.GetTransformedVertice(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    return Collision.IntersectCircles(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }
            return false;
        }

        public static bool IntersectCirclePolygon(FlatVector circleCentre, float circleRadius,FlatVector polygonCenter, FlatVector[] vertices, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;

            float axisDepth = 0;
            float minA, maxA, minB, maxB;
            FlatVector axis = FlatVector.Zero;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector va = vertices[i];
                FlatVector vb = vertices[(i + 1) % vertices.Length];

                FlatVector edge = vb - va;
                axis = new FlatVector(-edge.Y, edge.X);
                axis = FlatMath.Normalize(axis);
                Collision.ProjectVertices(vertices, axis, out minA, out maxA);
                Collision.ProjectCircle(circleCentre, circleRadius, axis, out minB, out maxB);

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
            axis = FlatMath.Normalize(axis);


            Collision.ProjectVertices(vertices, axis, out minA, out maxA);
            Collision.ProjectCircle(circleCentre, circleRadius, axis, out minB, out maxB);

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
        public static bool IntersectPolygons(FlatVector centerA, FlatVector[] verticesA,FlatVector centerB, FlatVector[] verticesB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector va = verticesA[i];
                FlatVector vb = verticesA[(i + 1) % verticesA.Length];

                FlatVector edge = vb - va;
                FlatVector axis = new FlatVector(-edge.Y, edge.X);
                axis = FlatMath.Normalize(axis);
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

            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector va = verticesB[i];
                FlatVector vb = verticesB[(i + 1) % verticesB.Length];

                FlatVector edge = vb - va;
                FlatVector axis = new FlatVector(-edge.Y, edge.X);
                axis = FlatMath.Normalize(axis);

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



            FlatVector direction = centerB - centerA;

            if (FlatMath.Dot(direction, normal) < 0f)
            {
                normal = -normal;
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
