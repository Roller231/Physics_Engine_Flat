using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flat;
using Flat.Graphics;
using Flat.Input;
using FlatPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using FlatMath = FlatPhysics.FlatMath;

namespace PhysicsEngine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Screen screen;  
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;

        private List<FlatBody> bodyList;
        private Color[] colors;

        private Vector2[] vertexBuffer;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.SynchronizeWithVerticalRetrace = true;

            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = true;

            const double UpdatePerSecond = 60d;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatePerSecond));
        }

        protected override void Initialize()
        {


            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

            this.screen = new Screen(this, 1280, 720);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.camera.Zoom = 24;

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);

            int bodyCount = 15;
            float padding = MathF.Abs(right - left) * 0.05f;
            this.bodyList = new List<FlatBody>(bodyCount);
            this.colors = new Color[bodyCount];


            for(int i = 0; i < bodyCount; i++)
            {
                int type = RandomHelper.RandomInteger(0, 2);
                type = (int)ShapeType.Box;

                FlatBody body = null;

                float x = RandomHelper.RandomSingle(left + padding, right - padding);
                float y = RandomHelper.RandomSingle(bottom + padding, top - padding);

                if(type == (int)ShapeType.Circle)
                {
                    if (!FlatBody.CreateCircleBody(1f, new FlatVector(x, y), 2f, false, 0.5f, out body, out string errorMessage))
                    {
                        throw new Exception();
                    }
                }    
                else if(type == (int)ShapeType.Box)
                {
                    if (!FlatBody.CreateBoxBody(1f, 1f, new FlatVector(x, y), 2f, false, 0.5f, out body, out string errorMessage))
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception("ink type");
                }

                this.bodyList.Add(body);
                this.colors[i] = RandomHelper.RandomColor();
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            FlatKeyboard keyboard = FlatKeyboard.Instance;
            FlatMouse mouse = FlatMouse.Instance;

            keyboard.Update();
            mouse.Update();

            if (keyboard.IsKeyAvailable)
            {
                if (keyboard.IsKeyClicked(Keys.Escape))
                {
                    this.Exit();
                }
                if (keyboard.IsKeyClicked(Keys.A))
                { 
                    this.camera.IncZoom();
                }
                if (keyboard.IsKeyClicked(Keys.Z))
                {
                    this.camera.DecZoom();
                }

                float dx = 0f;
                float dy = 0f;
                float speed = 15f;

                if(keyboard.IsKeyDown(Keys.Left)) { dx--; }
                if(keyboard.IsKeyDown(Keys.Right)) { dx++; }
                if(keyboard.IsKeyDown(Keys.Down)) { dy--; }
                if(keyboard.IsKeyDown(Keys.Up)) { dy++; }

                if(dx!=0 | dy != 0)
                {
                    FlatVector direction = FlatMath.Normalize(new FlatVector(dx, dy));
                    FlatVector velocity = direction * speed * FlatUtil.GetElapsedTimeInSeconds(gameTime);
                    this.bodyList[0].Move(velocity);
                }
            }

            for (int i = 0; i < this.bodyList.Count; i++)
            {
                FlatBody body = this.bodyList[i];
                body.Rotate(MathF.PI / 2f * FlatUtil.GetElapsedTimeInSeconds(gameTime));
            }

            //for(int i = 0; i < this.bodyList.Count - 1; i++)
            //{
            //    FlatBody bodyA = this.bodyList[i];

            //    for(int j = i + 1; j < this.bodyList.Count; j++)
            //    {
            //        FlatBody bodyB = this.bodyList[j];

            //        if (Collision.IntersectCircles(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius, out FlatVector normal, out float depth))
            //        {
            //            bodyA.Move(-normal * depth / 2f);
            //            bodyB.Move(normal * depth / 2f);
            //        }
            //    }
            //}


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(new Color(50,60,70));



            this.shapes.Begin(this.camera);


            for (int i = 0; i < this.bodyList.Count; i++)
            {
                FlatBody body = this.bodyList[i];
                Vector2 position = FlatConverter.ToVector2(body.Position);
                if(body.ShapeType is ShapeType.Circle)
                {
                    shapes.DrawCircleFill(position, body.Radius, 25, this.colors[i]);
                    shapes.DrawCircle(position, body.Radius, 25, Color.White);
                }
                else if (body.ShapeType is ShapeType.Box)
                {
                    //shapes.DrawBox(position, body.Width, body.Height, Color.White);
                    FlatConverter.ToVector2Array(body.GetTransformedVertice(), ref this.vertexBuffer);
                    shapes.DrawPolygonFill(this.vertexBuffer, body.Triangles, this.colors[i]);
                    shapes.DrawPolygon(this.vertexBuffer, Color.White);

                }
            }


            this.shapes.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);

            base.Draw(gameTime);
        }
    }
}
