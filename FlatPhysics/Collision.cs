﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{

    public static class Collision
    {
        public static void PointSegmentDistance(FlatVector p,  FlatVector a, FlatVector b, out float distanceSqured, out FlatVector cp )
        {
            FlatVector ab = b - a;
            FlatVector ap = p - a;

            float proj = FlatMath.Dot(ap,ab);
            float abLenSq = FlatMath.LengthSquared(ab);
            float d = proj  / abLenSq;

            if(d <= 0f)
            {
                cp = a;
            }
            else if (d >= 1f)
            {
                cp = b;
            }
            else 
            {
                cp = a + ab * d;
            }

            distanceSqured = FlatMath.DistanceSquared(p, cp);
        }


        public static bool IntersectAABBs(FlatAABB a, FlatAABB b)
        {
            if(a.Max.X <= b.Min.X || b.Max.X <= a.Min.X ||
                a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y)
            {
                return false;
            }
            return true;
        }

        public static void FindContactPoints(FlatBody bodyA, FlatBody bodyB, out FlatVector contact1, out FlatVector contact2, out int contactCount)
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
                    Collision.FindPolygonContactPoints(bodyA.GetTransformedVertice(), bodyB.GetTransformedVertice(),
                        out contact1, out contact2, out contactCount);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    Collision.FindCirclePolygonContactPoint(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformedVertice(), out  contact1);
                    contactCount = 1;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    Collision.FindCirclePolygonContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformedVertice(), out contact1);
                    contactCount = 1;
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    Collision.FindCirclesContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, out contact1);
                    contactCount = 1;
                }
            }
        }

        private static void FindPolygonContactPoints(
            FlatVector[] verticesA, FlatVector[] verticesB,
            out FlatVector contact1, out FlatVector contact2,
            out int contactCount)
        {
            contact1 = FlatVector.Zero;
            contact2 = FlatVector.Zero;
            contactCount = 0;

            float minDistSqr = float.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector p = verticesA[i];

                for(int j = 0; j < verticesB.Length; j++)
                {
                    FlatVector  va = verticesB[j];
                    FlatVector  vb = verticesB[(j+1) % verticesB.Length];

                    Collision.PointSegmentDistance(p, va, vb, out float distSq, out FlatVector cp);

                    if(FlatMath.NearlyEqual( distSq , minDistSqr))
                    {
                        if(!FlatMath.NearlyEqual( cp , contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if(distSq < minDistSqr)
                    {
                        minDistSqr = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector p = verticesB[i];

                for (int j = 0; j < verticesA.Length; j++)
                {
                    FlatVector va = verticesA[j];
                    FlatVector vb = verticesA[(j + 1) % verticesA.Length];

                    Collision.PointSegmentDistance(p, va, vb, out float distSq, out FlatVector cp);

                    if (FlatMath.NearlyEqual(distSq, minDistSqr))
                    {
                        if (!FlatMath.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSqr)
                    {
                        minDistSqr = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

        }

        private static void FindCirclePolygonContactPoint(
            FlatVector circleCenter, float circleRadius,
            FlatVector polygonCenter, FlatVector[]  polygonVerices,
            out FlatVector cp)
        {
            cp = FlatVector.Zero;

            float minDistanceSq = float.MaxValue;

            for(int i = 0; i < polygonVerices.Length; i++)
            {
                FlatVector va = polygonVerices[i];
                FlatVector vb = polygonVerices[(i + 1) % polygonVerices.Length];
                
                Collision.PointSegmentDistance(circleCenter, va, vb, out float distanceSq, out FlatVector  contact);

                if(distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    cp = contact;
                }
            }
        }

        private static  void FindCirclesContactPoint(FlatVector centerA, float radiusA, FlatVector centerB, out FlatVector cp)
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
