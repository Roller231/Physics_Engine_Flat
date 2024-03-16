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

        private List<FlatEntity> entityList;
        private List<FlatEntity> entityRemovalList;


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
            this.Window.Position = new Point(10, 40);


            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

            this.screen = new Screen(this, 1280, 720);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.camera.Zoom = 20;

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);

            this.entityList = new List<FlatEntity>();
            this.entityRemovalList = new List<FlatEntity>();

            this.world = new FlatWorld();

            float padding = MathF.Abs(right - left) * 0.10f;

            if(!FlatBody.CreateBoxBody(right - left - padding * 2, 3f,
                1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
            {
                throw new Exception(errorMessage);
            }
            groundBody.MoveTo(new FlatVector(0, -10));

            this.world.AddBody(groundBody);
            this.entityList.Add(new FlatEntity(groundBody, Color.DarkGreen));

            if (!FlatBody.CreateBoxBody(20f, 2f, 1f, true, 0.5f, out FlatBody ledgeBody1, out errorMessage))
            {
                throw new Exception(errorMessage);
            }

            ledgeBody1.MoveTo(new FlatVector(-10, 2.5f));
            ledgeBody1.Rotate(-MathHelper.TwoPi / 20f);

            this.world.AddBody(ledgeBody1);
            this.entityList.Add(new FlatEntity(ledgeBody1, Color.DarkMagenta));

            if (!FlatBody.CreateBoxBody(20f, 2f, 1f, true, 0.5f, out FlatBody ledgeBody2, out errorMessage))
            {
                throw new Exception(errorMessage);
            }
            ledgeBody2.MoveTo(new FlatVector(10, 8.5f));
            ledgeBody2.Rotate(MathHelper.TwoPi / 20f);

            this.world.AddBody(ledgeBody2);
            this.entityList.Add(new FlatEntity(ledgeBody2, Color.DarkSeaGreen));



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
                float width = RandomHelper.RandomSingle(2f, 3f);
                float height = RandomHelper.RandomSingle(2f, 3f);

                FlatVector mouseWorldPosition =
                    FlatConverter.ToFlatVecor( mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                this.entityList.Add(new FlatEntity(this.world, width, height, false, mouseWorldPosition));


            }


            //add circle on mouse click
            if (mouse.IsRightMouseButtonReleased())
            {
                float radius = RandomHelper.RandomSingle(1.25f, 1.5f);

                FlatVector mouseWorldPosition =
                    FlatConverter.ToFlatVecor(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

                this.entityList.Add(new FlatEntity(this.world, radius, false, mouseWorldPosition));

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

            

                this.timer.Restart();
            this.world.Step(FlatUtil.GetElapsedTimeInSeconds(gameTime), 20);
            this.timer.Stop();

            this.totalWorldStepTimer += this.timer.Elapsed.TotalMilliseconds;
            this.totalBodyCount += this.world.BodyCount;
            this.totalSampleCount++;


            this.camera.GetExtents(out _, out _, out float viewBottom, out _);

            this.entityRemovalList.Clear();

            for (int i = 0; i < this.entityList.Count; i++)
            {
                FlatEntity entity = this.entityList[i];
                FlatBody body = entity.Body;

                if (body.IsStatic) { continue; }

                FlatAABB box = body.GetAABB();

                if (box.Max.Y < viewBottom)
                {
                    this.entityRemovalList.Add(entity);
                }

            }

            for(int i = 0; i < this.entityRemovalList.Count; ++i)
            {
                FlatEntity entity = this.entityRemovalList[i];
                this.world.RemoveBody(entity.Body);
                this.entityList.Remove(entity);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(new Color(50,60,70));



            this.shapes.Begin(this.camera);


            for (int i = 0; i < this.entityList.Count; i++)
            {
                this.entityList[i].Draw(this.shapes);
                
            }

            //List<FlatVector> contactPoints = this.world?.contactPointsList;
            //for (int i = 0; i < contactpoints.count; i++)
            //{
            //    shapes.drawboxfill(flatconverter.tovector2(contactpoints[i]), 0.5f, 0.5f, color.orange);
            //}

            this.shapes.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);
             
            base.Draw(gameTime);
        }

    }
}
