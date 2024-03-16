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

        private List<Color> colors;
        private List<Color> outlineColors;

        private Vector2[] vertexBuffer;

        private Stopwatch timer = new Stopwatch();
        private Stopwatch sampleTimer;

        private double totalWorldStepTimer = 0d;
        private int totalBodyCount = 0;
        private int totalSampleCount = 0;

        private string worldStepTimeString = string.Empty;
        private string bodyCountString   = string.Empty;

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

            this.colors = new List<Color>();
            this.outlineColors = new List<Color>();

            this.world = new FlatWorld();

            float padding = MathF.Abs(right - left) * 0.10f;

            if(!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, new FlatVector(0, -10),
                1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
            {
                throw new Exception(errorMessage);
            }

            this.world.AddBody(groundBody);
            this.colors.Add(Color.DarkGreen);
            this.outlineColors.Add(Color.White);

            if (!FlatBody.CreateBoxBody(20f, 2f, new FlatVector(-10, 2.5f), 1f, true, 0.5f, out FlatBody ledgeBody1, out errorMessage))
            {
                throw new Exception(errorMessage);
            }

            ledgeBody1.Rotate(-MathHelper.TwoPi / 20f);

            this.world.AddBody(ledgeBody1);
            this.colors.Add(Color.DarkMagenta);
            this.outlineColors.Add(Color.White);

            if (!FlatBody.CreateBoxBody(20f, 2f, new FlatVector(10, 8.5f), 1f, true, 0.5f, out FlatBody ledgeBody2, out errorMessage))
            {
                throw new Exception(errorMessage);
            }

            ledgeBody2.Rotate(MathHelper.TwoPi / 20f);

            this.world.AddBody(ledgeBody2);
            this.colors.Add(Color.DarkOrange);
            this.outlineColors.Add(Color.White);



            this.timer = new Stopwatch();

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

            //add box on mouse click
            if (mouse.IsLeftMouseButtonReleased())
            {
                float width = RandomHelper.RandomSingle(1f, 2f);
                float height = RandomHelper.RandomSingle(1f, 2f);

                FlatVector mouseWorldPosition =
                    FlatConverter.ToFlatVecor( mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                if(!FlatBody.CreateBoxBody(width, height, mouseWorldPosition, 2f, false, 0.6f, out FlatBody body, out string errorMesage))
                {
                    throw new Exception(errorMesage);
                }

                this.world.AddBody(body);
                this.colors.Add(RandomHelper.RandomColor());
                this.outlineColors.Add(Color.White);
            }


            //add circle on mouse click
            if (mouse.IsRightMouseButtonReleased())
            {
                float radius = RandomHelper.RandomSingle(0.75f, 1.5f);

                FlatVector mouseWorldPosition =
                    FlatConverter.ToFlatVecor(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                if (!FlatBody.CreateCircleBody(radius, mouseWorldPosition, 2f, false, 0.6f, out FlatBody body, out string errorMesage))
                {
                    throw new Exception(errorMesage);
                }

                this.world.AddBody(body);
                this.colors.Add(RandomHelper.RandomColor());
                this.outlineColors.Add(Color.White);
            }

            if (keyboard.IsKeyAvailable)
            {
                if (keyboard.IsKeyClicked(Keys.L))
                {
                    Console.WriteLine($"Колличество тел: {this.world.BodyCount}");
                    Console.WriteLine($"Время шага: {Math.Round(this.timer.Elapsed.TotalMilliseconds, 4)}");
                    Console.WriteLine();
                }

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
            }

#if false
                float dx = 0f;
                float dy = 0f;

                float forceMagnitude = 48f;

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

            if(this.sampleTimer.Elapsed.TotalSeconds > 1d)
            {
                this.bodyCountString = Math.Round( this.totalBodyCount / (double)this.totalSampleCount).ToString();
                this.worldStepTimeString = Math.Round( this.totalWorldStepTimer / (double)this.totalSampleCount, 4).ToString();

                this.totalBodyCount = 0;
                this.totalWorldStepTimer = 0d;
                this.totalSampleCount = 0;
                this.sampleTimer.Restart();
            }
#endif
            

                this.timer.Restart();
            this.world.Step(FlatUtil.GetElapsedTimeInSeconds(gameTime), 20);
            this.timer.Stop();

            this.totalWorldStepTimer += this.timer.Elapsed.TotalMilliseconds;
            this.totalBodyCount += this.world.BodyCount;
            this.totalSampleCount++;


            this.camera.GetExtents(out _, out _, out float viewBottom, out _);

            for (int i = 0; i < this.world.BodyCount; i++)
            {
                if(!this.world.GetBody(i, out FlatBody body))
                {
                    throw new ArgumentOutOfRangeException();
                }

                FlatAABB box = body.GetAABB();

                if(box.Max.Y < viewBottom)
                {
                    this.world.RemoveBody(body);
                    this.colors.RemoveAt(i);
                    this.outlineColors.RemoveAt(i);
                }

            }


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
                    shapes.DrawCircle(position, body.Radius, 25, this.outlineColors[i]);
                }
                else if (body.ShapeType is ShapeType.Box)
                {
                    //shapes.DrawBox(position, body.Width, body.Height, Color.White);
                    FlatConverter.ToVector2Array(body.GetTransformedVertice(), ref this.vertexBuffer);
                    shapes.DrawPolygonFill(this.vertexBuffer, body.Triangles, this.colors[i]);
                    shapes.DrawPolygon(this.vertexBuffer, this.outlineColors[i]);

                }
            }

            List<FlatVector> contactPoints = this.world?.contactPointsList;
            for(int i = 0; i < contactPoints.Count; i++)
            {
                shapes.DrawBoxFill(FlatConverter.ToVector2( contactPoints[i]), 0.5f, 0.5f, Color.Orange);
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
