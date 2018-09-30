using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Snake_Sharp
{
    public class GraphicsEngine
    {
        // Declaration of engine parts
        private Main mainClass;
        private GameInput gameInputClass;

        // Just a random number generator
        private Random spawnPointGenerator = new Random();

        // Sizes and dimensions
        private ushort snakeSize = 16;
        private ushort pointToCatchSize = 16;

        private ushort stepToMove = 20;

        // Counters
        private ushort tailCounter;

        // Game colours
        private SolidBrush snakeColour = new SolidBrush(Color.Red);
        private SolidBrush pointToCatchColour = new SolidBrush(Color.CornflowerBlue);

        // Game objects location
        public List<Point> SnakeHolderPublic { get; set; } = new List<Point>();
        private Point snakeHead;
        private Point pointToCatch;

        // Canvas variables
        private Rectangle gameCanvasDimension;
        private Bitmap originalGameCanvas;
        private Bitmap staticCanvas;
        private Bitmap lastFrameCanvas;

        private enum GameState
        {
            gameRunning,
            gameOver
        }

        private GameState currentGameState = GameState.gameRunning;

        public void InitGraphics(Main _passedMainClass, GameInput _passedGameInput)
        {
            // Initialize engine parts
            mainClass = _passedMainClass;
            gameInputClass = _passedGameInput;

            // Get the game windows rectangle
            gameCanvasDimension = new Rectangle(0, 0, mainClass.ClientSize.Width, mainClass.ClientSize.Height);

            originalGameCanvas = new Bitmap(gameCanvasDimension.Width, gameCanvasDimension.Height);
            staticCanvas = new Bitmap(gameCanvasDimension.Width, gameCanvasDimension.Height);
            lastFrameCanvas = new Bitmap(gameCanvasDimension.Width, gameCanvasDimension.Height);

            // Start the keyboard listener
            gameInputClass.SetHook();
            gameInputClass.SetEngineReference(mainClass, this);

            // First time snake head location
            snakeHead.X = (20 * ((gameCanvasDimension.Width / 20) / 4)) + 2;
            snakeHead.Y = (20 * ((gameCanvasDimension.Height / 20) / 2)) + 2;

            // Set all game values to default
            DefaultGameState();

            // Create the ogirinal canvas
            CreateCanvas();

            // Initialization message
            Console.WriteLine(DateTime.Now.ToString("[yyyy-MM-dd - hh:mm:ss] ") + "Graphics Engine initialized");
        }

        private void CreateCanvas()
        {
            using (Graphics graphicsObject = Graphics.FromImage(originalGameCanvas))
            {
                // Draw the actual canvas
                graphicsObject.FillRectangle(new SolidBrush(Color.FromArgb(45, 45, 48)), new Rectangle(0, 0, gameCanvasDimension.Width, gameCanvasDimension.Height));

                // Area within the snake can move
                graphicsObject.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 40, gameCanvasDimension.Width, gameCanvasDimension.Height - 80));

                // Draw all of the text
                graphicsObject.DrawString("Level/Gefressen: " + (tailCounter.ToString() == "0" ? "" : tailCounter.ToString()), new Font("Comic Sans MS", 12, FontStyle.Bold), new SolidBrush(Color.White), new Point(10, 10));

                graphicsObject.DrawString("Escape für Menü, \"Q\" um zu starten", new Font("Comic Sans MS", 12, FontStyle.Bold), new SolidBrush(Color.White), new Point(gameCanvasDimension.Width - System.Windows.Forms.TextRenderer.MeasureText("Escape für Menü, \"Q\" um zu starten", new Font("Comic Sans MS", 12, FontStyle.Bold)).Width - 10, 10));

                graphicsObject.DrawString("Geschwindigkeit:", new Font("Comic Sans MS", 12, FontStyle.Bold), new SolidBrush(Color.White), new Point(10, gameCanvasDimension.Height - System.Windows.Forms.TextRenderer.MeasureText("Geschwindigkeit:", new Font("Comic Sans MS", 12, FontStyle.Regular)).Height - 10));
            }
        }

        public void UpdateCanvas(PaintEventArgs e)
        {
            using (Bitmap bufferBitmap = new Bitmap(originalGameCanvas))
            {
                using (Graphics graphicsObject = Graphics.FromImage(bufferBitmap))
                {
                    // Draw a grid on the canvas
                    DrawGrid(graphicsObject);

                    // Draw all parts of the snake
                    for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                    {
                        // Draw the head
                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                    }

                    // Draw the point to catch
                    graphicsObject.FillRectangle(pointToCatchColour, new Rectangle(pointToCatch.X, pointToCatch.Y, pointToCatchSize, pointToCatchSize));
                }

                e.Graphics.DrawImage(bufferBitmap, 0, 0);
            }
        }

        public void UpdatePlayerObjects(PaintEventArgs e)
        {
            using (Bitmap bufferBitmap = new Bitmap(originalGameCanvas))
            {
                using (Graphics graphicsObject = Graphics.FromImage(bufferBitmap))
                {
                    bool collisonPoint = CollisionTestPoint();

                    // Draw a grid on the canvas
                    DrawGrid(graphicsObject);

                    if (collisonPoint) { AddSnakeTail(); }

                    switch (gameInputClass.CurrentDirectionPublic)
                    {
                        case GameInput.SnakeDirection.Left:
                            bool collisionBorderLeft = CollisionTestBorder();
                            bool collisionSnakeLeft = CollisionTestSnake();

                            for (int i = SnakeHolderPublic.Count - 1; i >= 0; i--)
                            {
                                if (i == 0)
                                {
                                    if (collisionBorderLeft || collisionSnakeLeft)
                                    {
                                        // Draw the head
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisionBorderLeft || !collisionSnakeLeft)
                                    {
                                        // Update and draw the head's position
                                        SnakeHolderPublic[i] = new Point(SnakeHolderPublic[i].X - stepToMove, SnakeHolderPublic[i].Y);
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                                else if (i > 0)
                                {
                                    if (collisionBorderLeft || collisionSnakeLeft)
                                    {
                                        // Draw the tail
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisionBorderLeft || !collisionSnakeLeft)
                                    {
                                        // Update and draw the tail's position
                                        SnakeHolderPublic[i] = SnakeHolderPublic[i - 1];
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                            }

                            if (collisionBorderLeft || collisionSnakeLeft)
                            {
                                // Draw the point to catch
                                graphicsObject.FillRectangle(pointToCatchColour, new Rectangle(pointToCatch.X, pointToCatch.Y, pointToCatchSize, pointToCatchSize));

                                staticCanvas = new Bitmap(bufferBitmap);
                                mainClass.GameRunning = false;
                                currentGameState = GameState.gameOver;
                            }
                            break;
                        case GameInput.SnakeDirection.Right:
                            bool collisionBorderRight = CollisionTestBorder();
                            bool collisionSnakeRight = CollisionTestSnake();

                            for (int i = SnakeHolderPublic.Count - 1; i >= 0; i--)
                            {
                                if (i == 0)
                                {
                                    if (collisionBorderRight || collisionSnakeRight)
                                    {
                                        // Draw the head
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisionBorderRight || !collisionSnakeRight)
                                    {
                                        // Update and draw the head's position
                                        SnakeHolderPublic[i] = new Point(SnakeHolderPublic[i].X + stepToMove, SnakeHolderPublic[i].Y);
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                                else if (i > 0)
                                {
                                    if (collisionBorderRight || collisionSnakeRight)
                                    {
                                        // Draw the tail
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisionBorderRight || !collisionSnakeRight)
                                    {
                                        // Update and draw the tail's position
                                        SnakeHolderPublic[i] = SnakeHolderPublic[i - 1];
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                            }

                            if (collisionBorderRight || collisionSnakeRight)
                            {
                                // Draw the point to catch
                                graphicsObject.FillRectangle(pointToCatchColour, new Rectangle(pointToCatch.X, pointToCatch.Y, pointToCatchSize, pointToCatchSize));

                                staticCanvas = new Bitmap(bufferBitmap);
                                mainClass.GameRunning = false;
                                currentGameState = GameState.gameOver;
                            }
                            break;
                        case GameInput.SnakeDirection.Up:
                            bool collisionBorderUp = CollisionTestBorder();
                            bool collisionSnakeUp = CollisionTestSnake();

                            for (int i = SnakeHolderPublic.Count - 1; i >= 0; i--)
                            {
                                if (i == 0)
                                {
                                    if (collisionBorderUp || collisionSnakeUp)
                                    {
                                        // Draw the head
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisionBorderUp || !collisionSnakeUp)
                                    {
                                        // Update and draw the head's position
                                        SnakeHolderPublic[i] = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y - stepToMove);
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                                else if (i > 0)
                                {
                                    if (collisionBorderUp || collisionSnakeUp)
                                    {
                                        // Draw the tail
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisionBorderUp || !collisionSnakeUp)
                                    {
                                        // Update and draw the tail's position
                                        SnakeHolderPublic[i] = SnakeHolderPublic[i - 1];
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                            }

                            if (collisionBorderUp || collisionSnakeUp)
                            {
                                // Draw the point to catch
                                graphicsObject.FillRectangle(pointToCatchColour, new Rectangle(pointToCatch.X, pointToCatch.Y, pointToCatchSize, pointToCatchSize));

                                staticCanvas = new Bitmap(bufferBitmap);
                                mainClass.GameRunning = false;
                                currentGameState = GameState.gameOver;
                            }
                            break;
                        case GameInput.SnakeDirection.Down:
                            bool collisonBorderDown = CollisionTestBorder();
                            bool collisionSnakeDown = CollisionTestSnake();

                            for (int i = SnakeHolderPublic.Count - 1; i >= 0; i--)
                            {
                                if (i == 0)
                                {
                                    if (collisonBorderDown || collisionSnakeDown)
                                    {
                                        // Draw the head
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisonBorderDown || !collisionSnakeDown)
                                    {
                                        // Update and draw the head's position
                                        SnakeHolderPublic[i] = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y + stepToMove);
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                                else if (i > 0)
                                {
                                    if (collisonBorderDown || collisionSnakeDown)
                                    {
                                        // Draw the tail
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                    else if (!collisonBorderDown || !collisionSnakeDown)
                                    {
                                        // Update and draw the tail's position
                                        SnakeHolderPublic[i] = SnakeHolderPublic[i - 1];
                                        graphicsObject.FillRectangle(snakeColour, new Rectangle(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y, snakeSize, snakeSize));
                                    }
                                }
                            }

                            if (collisonBorderDown || collisionSnakeDown)
                            {
                                // Draw the point to catch
                                graphicsObject.FillRectangle(pointToCatchColour, new Rectangle(pointToCatch.X, pointToCatch.Y, pointToCatchSize, pointToCatchSize));

                                staticCanvas = new Bitmap(bufferBitmap);
                                mainClass.GameRunning = false;
                                currentGameState = GameState.gameOver;
                            }
                            break;
                        default:
                            break;
                    }

                    // Respawns the point if the next location of the head would be the point's location
                    // Draw the point to catch
                    graphicsObject.FillRectangle(pointToCatchColour, new Rectangle(pointToCatch.X, pointToCatch.Y, pointToCatchSize, pointToCatchSize));
                }
                // Draw the current frame
                lastFrameCanvas = new Bitmap(bufferBitmap);

                e.Graphics.DrawImage(bufferBitmap, 0, 0);
            }
        }

        public void DrawStaticCanvas(PaintEventArgs e)
        {
            switch (currentGameState)
            {
                case GameState.gameRunning:
                {
                    using (Bitmap bufferBitmap = new Bitmap(lastFrameCanvas))
                    {
                        using (Graphics graphicsObject = Graphics.FromImage(bufferBitmap))
                        {
                            int windowWidth = 400;
                            int windowHeight = 150;
                            int windowXPosition = (gameCanvasDimension.Width / 2) - (windowWidth / 2);
                            int windowYPosition = (gameCanvasDimension.Height / 2) - (windowHeight / 2);

                            Pen borderColour = new Pen(Color.FromArgb(0, 151, 251));

                            //graphicsObject.FillRectangle(new SolidBrush(Color.FromArgb(45, 45, 48)), new Rectangle(windowXPosition, windowYPosition, windowWidth, windowHeight));

                            //graphicsObject.DrawRectangle(borderColour, new Rectangle((gameCanvasDimension.Width / 2) - (windowWidth / 2), (gameCanvasDimension.Height / 2) - (windowHeight / 2), windowWidth, windowHeight));

                            // Draw all of the text
                        }

                        e.Graphics.DrawImage(bufferBitmap, 0, 0);
                    }
                }
                break;
                case GameState.gameOver:
                {
                    using (Bitmap bufferBitmap = new Bitmap(staticCanvas))
                    {
                        using (Graphics graphicsObject = Graphics.FromImage(bufferBitmap))
                        {
                            int windowWidth = 400;
                            int windowHeight = 150;
                            int windowXPosition = (gameCanvasDimension.Width / 2) - (windowWidth / 2);
                            int windowYPosition = (gameCanvasDimension.Height / 2) - (windowHeight / 2);

                            Pen borderColour = new Pen(Color.FromArgb(0, 151, 251));

                            graphicsObject.FillRectangle(new SolidBrush(Color.FromArgb(45, 45, 48)), new Rectangle(windowXPosition, windowYPosition, windowWidth, windowHeight));

                            graphicsObject.DrawRectangle(borderColour, new Rectangle((gameCanvasDimension.Width / 2) - (windowWidth / 2), (gameCanvasDimension.Height / 2) - (windowHeight / 2), windowWidth, windowHeight));

                            // Draw all of the text
                            graphicsObject.DrawString("Game Over!", new Font("Comic Sans MS", 12, FontStyle.Bold), new SolidBrush(Color.White), new Point(windowXPosition + ((windowWidth / 2) - (System.Windows.Forms.TextRenderer.MeasureText("Game Over!", new Font("Comic Sans MS", 12, FontStyle.Bold)).Width / 2)), windowYPosition + 30));

                            graphicsObject.DrawString("Level/gefressen: " + tailCounter.ToString(), new Font("Comic Sans MS", 12, FontStyle.Bold), new SolidBrush(Color.White), new Point(windowXPosition + ((windowWidth / 2) - (System.Windows.Forms.TextRenderer.MeasureText("Level/gefressen: " + tailCounter.ToString(), new Font("Comic Sans MS", 12, FontStyle.Bold)).Width / 2)), windowYPosition + 60));

                            graphicsObject.DrawString("Um neu zu starten drücken Sie \"Q\"", new Font("Comic Sans MS", 12, FontStyle.Bold), new SolidBrush(Color.White), new Point(windowXPosition + ((windowWidth / 2) - (System.Windows.Forms.TextRenderer.MeasureText("Um neu zu starten drücken Sie \"Q\"", new Font("Comic Sans MS", 12, FontStyle.Bold)).Width / 2)), windowYPosition + 90));
                        }

                        e.Graphics.DrawImage(bufferBitmap, 0, 0);
                    }
                }
                break;
                default:
                {
                }
                break;
            }
        }

        private void AddSnakeTail()
        {
            tailCounter++;
            CreateCanvas();
            SnakeHolderPublic.Add(SnakeHolderPublic[SnakeHolderPublic.Count - 1]);
        }

        private bool CollisionTestPoint()
        {
            // Set new position for the point to catch
            bool noSnakeInterfere = true;

            Point newTailPoint = new Point(0, 0);
            Point currentTailPoint = new Point(0, 0);


            switch (gameInputClass.CurrentDirectionPublic)
            {
                case GameInput.SnakeDirection.Left:
                    Point nextLocationLeft = new Point(SnakeHolderPublic[0].X - 20, SnakeHolderPublic[0].Y);

                    if (nextLocationLeft == pointToCatch)
                    {
                        while (true)
                        {
                            noSnakeInterfere = true;
                            newTailPoint = new Point((20 * spawnPointGenerator.Next(1, (gameCanvasDimension.Width / 20))) + 2, (20 * spawnPointGenerator.Next(2, (gameCanvasDimension.Height / 20) - 2)) + 2);

                            for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                            {
                                currentTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                                if (currentTailPoint == newTailPoint)
                                {
                                    noSnakeInterfere = false;
                                    break;
                                }
                            }

                            if (noSnakeInterfere)
                            {
                                pointToCatch = newTailPoint;
                                return true;
                            }
                        }
                    }
                    break;
                case GameInput.SnakeDirection.Right:
                    Point nextLocationRight = new Point(SnakeHolderPublic[0].X + 20, SnakeHolderPublic[0].Y);

                    if (nextLocationRight == pointToCatch)
                    {
                        while (true)
                        {
                            noSnakeInterfere = true;
                            newTailPoint = new Point((20 * spawnPointGenerator.Next(1, (gameCanvasDimension.Width / 20))) + 2, (20 * spawnPointGenerator.Next(2, (gameCanvasDimension.Height / 20) - 2)) + 2);

                            for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                            {
                                currentTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                                if (currentTailPoint == newTailPoint)
                                {
                                    noSnakeInterfere = false;
                                    break;
                                }
                            }

                            if (noSnakeInterfere)
                            {
                                pointToCatch = newTailPoint;
                                return true;
                            }
                        }
                    }
                    break;
                case GameInput.SnakeDirection.Up:
                    Point nextLocationUp = new Point(SnakeHolderPublic[0].X, SnakeHolderPublic[0].Y - 20);

                    if (nextLocationUp == pointToCatch)
                    {
                        while (true)
                        {
                            noSnakeInterfere = true;
                            newTailPoint = new Point((20 * spawnPointGenerator.Next(1, (gameCanvasDimension.Width / 20))) + 2, (20 * spawnPointGenerator.Next(2, (gameCanvasDimension.Height / 20) - 2)) + 2);

                            for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                            {
                                currentTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                                if (currentTailPoint == newTailPoint)
                                {
                                    noSnakeInterfere = false;
                                    break;
                                }
                            }

                            if (noSnakeInterfere)
                            {
                                pointToCatch = newTailPoint;
                                return true;
                            }
                        }
                    }
                    break;
                case GameInput.SnakeDirection.Down:
                    Point nextLocationDown = new Point(SnakeHolderPublic[0].X, SnakeHolderPublic[0].Y + 20);

                    if (nextLocationDown == pointToCatch)
                    {
                        while (true)
                        {
                            noSnakeInterfere = true;
                            newTailPoint = new Point((20 * spawnPointGenerator.Next(1, (gameCanvasDimension.Width / 20))) + 2, (20 * spawnPointGenerator.Next(2, (gameCanvasDimension.Height / 20) - 2)) + 2);

                            for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                            {
                                currentTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                                if (currentTailPoint == newTailPoint)
                                {
                                    noSnakeInterfere = false;
                                    break;
                                }
                            }

                            if (noSnakeInterfere)
                            {
                                pointToCatch = newTailPoint;
                                return true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

        private bool CollisionTestBorder()
        {
            switch (gameInputClass.CurrentDirectionPublic)
            {
                case GameInput.SnakeDirection.Left:
                    if ((SnakeHolderPublic[0].X - stepToMove) < 0)
                    {

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case GameInput.SnakeDirection.Right:
                    if ((SnakeHolderPublic[0].X + stepToMove) > gameCanvasDimension.Width)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case GameInput.SnakeDirection.Up:
                    if ((SnakeHolderPublic[0].Y - stepToMove) <= 40)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case GameInput.SnakeDirection.Down:
                    if ((SnakeHolderPublic[0].Y + stepToMove) >= (gameCanvasDimension.Height - 40))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    break;
            }

            return false;
        }
        
        private bool CollisionTestSnake()
        {
            Point snakeHeadLocation = new Point(0, 0);
            Point nextSankeTailPoint = new Point(0, 0);

            // Check if the head is touching a tail-pics
            switch (gameInputClass.CurrentDirectionPublic)
            {
                case GameInput.SnakeDirection.Left:
                    snakeHeadLocation = new Point(SnakeHolderPublic[0].X - 20, SnakeHolderPublic[0].Y);

                    for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                    {
                        if (i > 3)
                        {
                            nextSankeTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                            if (snakeHeadLocation == nextSankeTailPoint)
                            {
                                //MessageBox.Show("Tail: " + (i + 1) + "\nCurrent Position:" + snakeHeadLocation + " - New Position: " + nextSankeTailPoint);
                                return true;
                            }
                        }
                    }
                    break;
                case GameInput.SnakeDirection.Right:
                    snakeHeadLocation = new Point(SnakeHolderPublic[0].X + 20, SnakeHolderPublic[0].Y);

                    for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                    {
                        if (i > 3)
                        {
                            nextSankeTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                            if (snakeHeadLocation == nextSankeTailPoint)
                            {
                                //MessageBox.Show("Tail: " + (i + 1) + "\nCurrent Position:" + snakeHeadLocation + " - New Position: " + nextSankeTailPoint);
                                return true;
                            }
                        }
                    }
                    break;
                case GameInput.SnakeDirection.Up:
                    snakeHeadLocation = new Point(SnakeHolderPublic[0].X, SnakeHolderPublic[0].Y - 20);

                    for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                    {
                        if (i > 3)
                        {
                            nextSankeTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                            if (snakeHeadLocation == nextSankeTailPoint)
                            {
                                //MessageBox.Show("Tail: " + (i + 1) + "\nCurrent Position:" + snakeHeadLocation + " - New Position: " + nextSankeTailPoint);
                                return true;
                            }
                        }
                    }
                    break;
                case GameInput.SnakeDirection.Down:
                    snakeHeadLocation = new Point(SnakeHolderPublic[0].X, SnakeHolderPublic[0].Y + 20);

                    for (ushort i = 0; i < SnakeHolderPublic.Count; i++)
                    {
                        if (i > 3)
                        {
                            nextSankeTailPoint = new Point(SnakeHolderPublic[i].X, SnakeHolderPublic[i].Y);

                            if (snakeHeadLocation == nextSankeTailPoint)
                            {
                                //MessageBox.Show("Tail: " + (i + 1) + "\nCurrent Position:" + snakeHeadLocation + " - New Position: " + nextSankeTailPoint);
                                return true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

        private void DrawGrid(Graphics g)
        {
            // Draw the vertical lines
            for (ushort i = 0; i <= (mainClass.ClientSize.Width / 20); i++)
            {
                g.DrawLine(Pens.White, new Point(i * 20, 40), new Point(i * 20, mainClass.ClientSize.Height - 41));
                g.DrawLine(Pens.White, new Point((i * 20) - 1, 40), new Point((i * 20) - 1, mainClass.ClientSize.Height - 41));
            }

            // Draw the horizontal lines
            for (ushort i = 0; i <= (mainClass.ClientSize.Height / 20); i++)
            {
                // Upper line
                if (i > 1 && i < (mainClass.ClientSize.Height / 20) - 2)
                {
                    g.DrawLine(Pens.White, new Point(0, i * 20), new Point(mainClass.ClientSize.Width, i * 20));
                }

                // Upper line
                if (i > 2 && i < (mainClass.ClientSize.Height / 20) - 1)
                {
                    // Bottom line
                    g.DrawLine(Pens.White, new Point(0, (i * 20) - 1), new Point(mainClass.ClientSize.Width, (i * 20) - 1));
                }
            }
        }

        public void DefaultGameState()
        {
            // Reseting everything and spawn a new point to catch/tail-pics
            SnakeHolderPublic.Clear();

            pointToCatch.X = (20 * spawnPointGenerator.Next(1, (gameCanvasDimension.Width / 20))) + 2;
            pointToCatch.Y = (20 * spawnPointGenerator.Next(2, (gameCanvasDimension.Height / 20) - 2)) + 2;

            SnakeHolderPublic.Add(snakeHead);


            gameInputClass.CurrentDirectionPublic = GameInput.SnakeDirection.Right;

            // Reset the counter and draw it
            tailCounter = 0;
            CreateCanvas();

            currentGameState = GameState.gameRunning;

            if (!mainClass.GameRunning) { mainClass.GameRunning = true; }
        }
    }
}