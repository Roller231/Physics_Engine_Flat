using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatPhysics
{
    public sealed class FlatWorld
    {
        public static readonly float MinBodySize = 0.01f * 0.01f;
        public static readonly float MaxBodySize = 64f * 64f;

        public static readonly float MinDensity = 0.2f; //g/cm^3
        public static readonly float MaxDensity = 22.6f; //g/cm^3

        private List<FlatBody> bodyList;
        private FlatVector gravity;

        public int BodyCount
        {
            get { return bodyList.Count; }
        }

        public  FlatWorld()
        {
            this.gravity = new FlatVector(0f, 9.81f);
            this.bodyList = new List<FlatBody>();
        }

        public void AddBody(FlatBody body)
        {
            this.bodyList.Add(body);
        }

        public bool RemoveBody(FlatBody  body)
        {
            return this.bodyList.Remove(body);
        }

        public bool GetBody(int index, out FlatBody body)
        {
            body = null;

            if(index < 0 || index >= this.bodyList.Count)
            {
                return false;
            }
            body = this.bodyList[index];
            return true;
        }

        public void Step(float time)
        {
            //Шаг перемещения
            for (int i = 0; i < this.bodyList.Count; i++)
            {
                this.bodyList[i].Step(time);
            }

            //Шаг коллизий
            for (int i = 0; i < this.bodyList.Count - 1; i++)
            {
                FlatBody bodyA = this.bodyList[i];

                for (int j = i + 1; j < this.bodyList.Count; j++)
                {
                    FlatBody bodyB = this.bodyList[j];

                    if(this.Collide(bodyA, bodyB, out FlatVector normal, out float depth))
                    {
                        bodyA.Move(-normal * depth / 2f);
                        bodyB.Move(normal * depth / 2f);

                        this.ResolveCollision(bodyA, bodyB, normal,depth);
                    }
                }
            }
        }

        public void ResolveCollision(FlatBody bodyA, FlatBody bodyB, FlatVector normal, float depth)
        {
            FlatVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float j = -(1f + e) * FlatMath.Dot(relativeVelocity, normal);
            j /= (1f / bodyA.Mass) + (1f / bodyB.Mass);


            bodyA.LinearVelocity -= j / bodyA.Mass * normal;
            bodyB.LinearVelocity += j / bodyB.Mass * normal;
        }

        public bool Collide(FlatBody bodyA, FlatBody bodyB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0;

            ShapeType  shapeTypeA = bodyA.ShapeType;
            ShapeType  shapeTypeB = bodyB.ShapeType;

            if(shapeTypeA is ShapeType.Box)
            {
                if(shapeTypeB is ShapeType.Box)
                {
                    return Collision.IntersectPolygons(bodyA.GetTransformedVertice(), bodyB.GetTransformedVertice(),
                        out normal, out depth);
                }
                else if(shapeTypeB is ShapeType.Circle)
                {
                    bool result =  Collision.IntersectCirclePolygon(bodyB.Position, bodyB.Radius, bodyA.GetTransformedVertice(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if(shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collision.IntersectCirclePolygon(bodyA.Position, bodyA.Radius, bodyB.GetTransformedVertice(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    return Collision.IntersectCircles(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }
            return false;
        }

    }
}
