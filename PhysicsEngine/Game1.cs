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

        //public float rx;

        private GraphicsDeviceManager graphics;
        private Screen screen;  
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;

        private FlatWorld world;

        private Color[] colors;
        private Color[] outlineColors;

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

            this.world = new FlatWorld();

            this.colors = new Color[bodyCount];
            this.outlineColors = new Color[bodyCount];


            for(int i = 0; i < bodyCount; i++)
            {
                int type = RandomHelper.RandomInteger(0, 2);

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
                    if (!FlatBody.CreateBoxBody(2f, 2f, new FlatVector(x, y), 2f, false, 0.5f, out body, out string errorMessage))
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    throw new Exception("ink type");
                }

                this.world.AddBody(body);
                this.colors[i] = RandomHelper.RandomColor();
                this.outlineColors[i] = Color.White;
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

                float forceMagnitude = 15f;

                if(keyboard.IsKeyDown(Keys.Left)) { dx--; }
                if(keyboard.IsKeyDown(Keys.Right)) { dx++; }
                if(keyboard.IsKeyDown(Keys.Down)) { dy--; }
                if(keyboard.IsKeyDown(Keys.Up)) { dy++; }
                //if (keyboard.IsKeyDown(Keys.A)) { rx++; }

                if(!this.world.GetBody(0, out FlatBody body))
                {
                    throw new Exception("Не найдено тело с таким индексом.");
                }

                if (dx!=0 | dy != 0)
                {
                    FlatVector forceDirection = FlatMath.Normalize(new FlatVector(dx, dy));
                    FlatVector force = forceDirection * forceMagnitude;

                    body.AddForce(force);
                }
            }

            this.world.Step(FlatUtil.GetElapsedTimeInSeconds(gameTime));

            this.WrapScreen();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(new Color(50,60,70));



            this.shapes.Begin(this.camera);


            for (int i = 0; i < this.world.BodyCount; i++)
            {
                if(!this.world.GetBody(i, out FlatBody body))
                {
                    throw new Exception("Не найдено тело с таким индексом.");
                }
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
                    shapes.DrawPolygon(this.vertexBuffer, this.outlineColors[i]);

                }
            }


            this.shapes.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);

            base.Draw(gameTime);
        }

        private void WrapScreen()
        {
            this.camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

            float viewWidth = camMax.X - camMin.X;
            float viewHeight = camMax.Y - camMin.Y;


            for(int i = 0; i < this.world.BodyCount; i++)
            {
                if(!this.world.GetBody(i, out FlatBody body))
                {
                    throw new Exception("");
                }
                if(body.Position.X < camMin.X) { body.MoveTo(body.Position + new FlatVector(viewWidth, 0f)); }
                if (body.Position.X > camMax.X) { body.MoveTo(body.Position - new FlatVector(viewWidth, 0f)); }
                if (body.Position.Y < camMin.Y) { body.MoveTo(body.Position + new FlatVector(0f, viewHeight)); }
                if (body.Position.Y > camMax.Y) { body.MoveTo(body.Position - new FlatVector(0f, viewHeight)); }

            }

        }
    }
}
