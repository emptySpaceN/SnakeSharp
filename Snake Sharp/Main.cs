using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace Snake_Sharp
{
    public partial class Main : Form
    {
        // Declaration of engine parts
        private GraphicsEngine Graphics_Engine;
        private GameInput Game_Input;

        // The canvas is painted if this bool is true
        public bool GameRunning { get; set; } = false;

        private Stopwatch frameMeasurement = new Stopwatch();

        // Game loop objects
        public Timer MainGameLoop { get; set; }

        // Time in milliseconds after the game objects are redrawn
        private ushort gameSpeed = 50;

        public Main()
        {
            InitializeComponent();

            // Events
            this.Paint += new PaintEventHandler(MainClass_Paint);
            this.Load += new EventHandler(MainClass_Load);
            this.FormClosing += new FormClosingEventHandler(MainClass_FormClosing);
        }

        private void MainClass_FormClosing(object sender, FormClosingEventArgs e)
        {
            Game_Input.UnHook();
        }

        private void MainClass_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.ClientSize = new System.Drawing.Size(20 * 50, 20 * 30);

            InitializeGame();
        }



        private void InitializeGame()
        {
            // Initialize engine parts
            Graphics_Engine = new GraphicsEngine();
            Game_Input = new GameInput();

            // Run engine parts for the first time
            Graphics_Engine.InitGraphics(this, Game_Input);

            // 
            MainGameLoop = new Timer();
            MainGameLoop.Interval = 1;
            MainGameLoop.Tick += new EventHandler(MainGameLoop_Tick);

            // Starting a new game loop and frame measurement
            MainGameLoop.Start();
            frameMeasurement.Start();
            GameRunning = true;
        }

        private void MainGameLoop_Tick(object sender, EventArgs e)
        {
            // The "game loop" calls the canvas paint event on every tick
            this.Invalidate();
            this.Update();
        }

        private void MainClass_Paint(object sender, PaintEventArgs e)
        {
            //if (Game_Input.restartGamePublic)
            //{

            //    Game_Input.restartGamePublic = false;
            //    Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Game restart");
            //    return;
            //}

            if (GameRunning && Game_Input.IsApplicationActivated())
            {
                e.Graphics.Clear(Color.Black);
                Graphics_Engine.UpdateCanvas(e);

                if (frameMeasurement.ElapsedMilliseconds >= gameSpeed)
                {
                    e.Graphics.Clear(Color.Black);
                    Graphics_Engine.UpdatePlayerObjects(e);

                    frameMeasurement.Restart();
                    Game_Input.ResetKeystate();
                }
                //Console.WriteLine("game running");
            }
            else
            {
                e.Graphics.Clear(Color.Black);
                Graphics_Engine.DrawStaticCanvas(e);
                //Console.WriteLine("game not running");
                //MessageBox.Show("asdasd");
                // Paint the last active frame - save the last frame to a bitmap and draw it continuesly till the game resume or restarts
            }

            //gameRunning = Graphics_Engine.gameRunningPublic;
        }
    }
}
