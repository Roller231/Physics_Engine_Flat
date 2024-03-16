using Flat;
using Flat.Graphics;
using FlatPhysics;
using System;
using Microsoft.Xna.Framework;


namespace PhysicsEngine
{
    public sealed class FlatEntity
    {
        public readonly FlatBody Body;
        public readonly Color Color;

        public FlatEntity(FlatBody body)
        {
            this.Body = body;
            this.Color = RandomHelper.RandomColor();
        }
        public FlatEntity(FlatBody body, Color color)
        {
            this.Body = body;
            this.Color = color;
        }

        public FlatEntity(FlatWorld world, float radius, bool isStatic, FlatVector position)
        {
            if(!FlatBody.CreateCircleBody(radius, 1f, isStatic, 0.5f, out FlatBody body, out string errorMessage))
            {
                throw new Exception(errorMessage);
            }

            body.MoveTo(position);
            this.Body = body;
            world.AddBody(body);
            this.Color = RandomHelper.RandomColor();
        }

        public FlatEntity(FlatWorld world, float width, float height, bool isStatic, FlatVector position)
        {
            if (!FlatBody.CreateBoxBody(width, height, 1f, isStatic, 0.5f, out FlatBody body, out string errorMessage))
            {
                throw new Exception(errorMessage);
            }

            body.MoveTo(position);
            this.Body = body;
            world.AddBody(body);
            this.Color = RandomHelper.RandomColor();

        }

        public void Draw(Shapes shapes)
        {
            Vector2 position = FlatConverter.ToVector2(this.Body.Position);

            if (this.Body.ShapeType == ShapeType.Circle)
            {
                Vector2 va = Vector2.Zero;
                Vector2 vb = new Vector2(Body.Radius, 0f);
                Flat.FlatTransform transform = new Flat.FlatTransform(position, Body.Angle, 1);
                va = Flat.FlatUtil.Transform(va, transform);
                vb = Flat.FlatUtil.Transform(vb, transform);

                shapes.DrawCircleFill(position, this.Body.Radius, 25, this.Color);
                shapes.DrawCircle(position, this.Body.Radius, 25, Color.White);
                shapes.DrawLine(va, vb, Color.White);
            }
            else if (this.Body.ShapeType == ShapeType.Box)
            {
                shapes.DrawBoxFill(position, this.Body.Width, this.Body.Height, this.Body.Angle, this.Color);
                shapes.DrawBox(position, this.Body.Width, this.Body.Height, this.Body.Angle, Color.White);
            }
        }
    }
}
