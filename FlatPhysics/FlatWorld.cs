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
        public static readonly float MaxDensity = 22.6f; //g/cm^3\

        public static readonly int MinIterations = 1;
        public static readonly int ManIterations = 128;

        private List<FlatBody> bodyList;
        private List<(int, int)> contactpairs;
        private FlatVector gravity;

        //public List<FlatVector> contactPointsList;

        public int BodyCount
        {
            get { return bodyList.Count; }
        }

        public  FlatWorld()
        {
            this.gravity = new FlatVector(0f, -9.81f);
            this.bodyList = new List<FlatBody>();
            this.contactpairs = new List<(int, int)> ();

            //this.contactPointsList = new List<FlatVector> ();
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

        public void Step(float time, int totalIterations)
        {
            totalIterations = FlatMath.Clamp(totalIterations, FlatWorld.MinIterations, FlatWorld.ManIterations);

            //this.contactPointsList.Clear();

            for (int currentIteration = 0; currentIteration < totalIterations; currentIteration++)
            {

                this.contactpairs.Clear();

                this.StepBodies(time, totalIterations);
                this.BroadPhase();
                this.NarrowPhase();

            }
        }

        public void BroadPhase()
        {
            //Шаг коллизий
            for (int i = 0; i < this.bodyList.Count - 1; i++)
            {
                FlatBody bodyA = this.bodyList[i];
                FlatAABB bodyA_aabb = bodyA.GetAABB();

                for (int j = i + 1; j < this.bodyList.Count; j++)
                {
                    FlatBody bodyB = this.bodyList[j];
                    FlatAABB bodyB_aabb = bodyB.GetAABB();

                    if (bodyA.IsStatic && bodyB.IsStatic)
                    {
                        continue;
                    }

                    if (!Collision.IntersectAABBs(bodyA_aabb, bodyB_aabb))
                    {
                        continue;
                    }


                    this.contactpairs.Add((i, j));
                }
            }
        }

        public void NarrowPhase()
        {

            for (int i = 0; i < this.contactpairs.Count; i++)
            {
                (int, int) pair = this.contactpairs[i];
                FlatBody bodyA = this.bodyList[pair.Item1];
                FlatBody bodyB = this.bodyList[pair.Item2];

                if (Collision.Collide(bodyA, bodyB, out FlatVector normal, out float depth))
                {
                    this.SeparateBodies(bodyA, bodyB, normal * depth);
                    Collision.FindContactPoints(bodyA, bodyB, out FlatVector contact1, out FlatVector contact2, out int contactCount);
                    FlatManifold contact = new FlatManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);

                    this.ResolveCollision(in contact);
                }


                //(DEBUG)
                //if (currentIteration == totalIterations - 1)
                //{
                //    if (!this.contactPointsList.Contains(contact.Contact1))
                //    {
                //        this.contactPointsList.Add(contact.Contact1);
                //    }


                //    if (contact.ContactCount > 1)
                //    {
                //        if (!this.contactPointsList.Contains(contact.Contact2))
                //        {
                //            this.contactPointsList.Add(contact.Contact2);
                //        }
                //    }
                //}
            }
        }

        public void StepBodies(float time, int totalIterations)
        {
            //Шаг перемещения
            for (int i = 0; i < this.bodyList.Count; i++)
            {
                this.bodyList[i].Step(time, this.gravity, totalIterations);
            }
        }

        private void SeparateBodies(FlatBody  bodyA, FlatBody bodyB, FlatVector mtv)
        {
            if (bodyA.IsStatic)
            {
                bodyB.Move(mtv);
            }
            else if (bodyB.IsStatic)
            {
                bodyA.Move(-mtv);
            }
            else
            {
                bodyA.Move(-mtv / 2f);
                bodyB.Move(mtv / 2f);
            }
        }

        public void ResolveCollision(in FlatManifold contact)
        {
            FlatBody bodyA = contact.BodyA;
            FlatBody bodyB = contact.BodyB;
            FlatVector normal = contact.Normal;
            float depth = contact.Depth;

            FlatVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if(FlatMath.Dot(relativeVelocity, normal) > 0)
            {
                return;
            }

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float j = -(1f + e) * FlatMath.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;

            FlatVector impulse = j * normal;

            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }



    }
}
