// Welcome to MultiSnake

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

class MultiSnakeBeta
{
    static void Main(string[] args)
    {
        ConfigureConsole();
        WelcomeScreen();

        while (playGame)
        {
            //Generate Snake
            Queue<Position> firstSnake = GenerateSnake();
            Queue<Position> secondSnake = GenerateSnake();

            //Generate Food
            Random foodGenerator = FoodGenerator();

            int lastFoodTime = 0;
            int foodDissapearTime = 8000;

            //Set Snake's speed
            double snakeSpeed = SetSpeed(100);

            //Give Snake Directions
            Position[] snakeDirections = SnakeDirections();

            // Current direction
            int currentDirection = 0; // 0 = right, 1 = down, 2 = left, 3 = up
            int secondDirection = 0;

            Position food = Food(foodGenerator);

            //Generate Obstacles
            List<Position> obstacles = GenerateObstacle();
            if (GameMode == 1)
            {
                InitialiseObstacles(obstacles);

                foreach (Position obstacle in obstacles)
                {
                    Console.SetCursorPosition(obstacle.X, obstacle.Y);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("X");
                }
            }

            // Basic info
            bool headSmash = false;
            int winner = 0;
            int firstSnakeMinus = 0;
            int secondSnakeMinus = 0;
            int snake1Start = 5;
            int snake2Start = Console.WindowHeight - 5;

            //Initialise Snake
            InitialiseSnake(firstSnake, 6, snake1Start, ConsoleColor.Cyan);
            InitialiseSnake(secondSnake, 6, snake2Start, ConsoleColor.Red);

            int firstScore = firstSnake.Count - firstSnakeMinus;
            int secondScore = secondSnake.Count - secondSnakeMinus;

            DrawScoreBoard();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                    if (pressedKey.Key == ConsoleKey.RightArrow)
                        if (currentDirection != 2) currentDirection = 0;
                    if (pressedKey.Key == ConsoleKey.DownArrow)
                        if (currentDirection != 3) currentDirection = 1;
                    if (pressedKey.Key == ConsoleKey.LeftArrow)
                        if (currentDirection != 0) currentDirection = 2;
                    if (pressedKey.Key == ConsoleKey.UpArrow)
                        if (currentDirection != 1) currentDirection = 3;

                    if (pressedKey.Key == ConsoleKey.D)
                        if (secondDirection != 2) secondDirection = 0;
                    if (pressedKey.Key == ConsoleKey.S)
                        if (secondDirection != 3) secondDirection = 1;
                    if (pressedKey.Key == ConsoleKey.A)
                        if (secondDirection != 0) secondDirection = 2;
                    if (pressedKey.Key == ConsoleKey.W)
                        if (secondDirection != 1) secondDirection = 3;

                    if (pressedKey.Key == ConsoleKey.Spacebar) { PauseGame(); }
                }

                // Initialise food
                Console.SetCursorPosition(food.X, food.Y);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("+");

                // Previous snake head
                Position prevFirstHead = firstSnake.Last();
                Position prevSecondHead = secondSnake.Last();

                // New snake head
                Position newFirstHead = NewSnakeHead(snakeDirections, currentDirection, ref prevFirstHead);
                Position newSecondHead = NewSnakeHead(snakeDirections, secondDirection, ref prevSecondHead);

                //Check for inpact.
                if (newSecondHead.X >= Console.WindowWidth ||
                    newSecondHead.X < 0 ||
                    newSecondHead.Y >= Console.WindowHeight ||
                    newSecondHead.Y < 3)
                {
                    winner = 1;
                    break;
                }
                else if (newFirstHead.X >= Console.WindowWidth ||
                    newFirstHead.X < 0 ||
                    newFirstHead.Y >= Console.WindowHeight ||
                    newFirstHead.Y < 3)
                {
                    winner = 2;
                    break;
                }
                if (firstScore == 25)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine();
                    Console.WriteLine("Congratulations {0} - you have been granted the title: Snake Ninja Master", Player1Name);
                    winner = 1;
                    break;
                }
                if (secondScore == 25)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine();
                    Console.WriteLine("Congratulations {0} - you have been granted the title: Snake Ninja Master", Player2Name);
                    winner = 2;
                    break;
                }

                // Check if their heads collide            
                if ((prevFirstHead.X == newSecondHead.X &&
                    prevFirstHead.Y == newSecondHead.Y &&
                    prevSecondHead.X == newFirstHead.X &&
                    prevSecondHead.Y == newFirstHead.Y) && (GameMode == 1))
                {
                    headSmash = true;
                    if (firstScore > secondScore)
                    {
                        winner = 1;
                    }
                    else
                    {
                        winner = 2;
                    }
                    break;
                }

                if (firstScore > 4 && GameMode == 1)
                {
                    if (Teleport(secondSnake, ref firstSnake, obstacles, snakeDirections, snake1Start, firstScore,
                        ref prevFirstHead, ref newFirstHead, ConsoleColor.Cyan))
                    {
                        currentDirection = 0;
                    }
                }
                // Second snake is hitting the first
                if (secondScore > 4 && GameMode == 1)
                {
                    if (Teleport(firstSnake, ref secondSnake, obstacles, snakeDirections, snake2Start, secondScore,
                        ref prevSecondHead, ref newSecondHead, ConsoleColor.Red))
                    {
                        secondDirection = 0;
                    }
                }

                // Write new snake head            
                WriteNewHead(prevFirstHead, ConsoleColor.Cyan);
                WriteNewHead(prevSecondHead, ConsoleColor.Red);

                // Add new snake element and draw it on the console
                newFirstHead = AddNewSnakeElem(firstSnake, newFirstHead, ConsoleColor.Cyan);
                newSecondHead = AddNewSnakeElem(secondSnake, newSecondHead, ConsoleColor.Red);

                // Check if the snake is on food
                CheckIfOnFood(firstSnake, obstacles, foodGenerator, lastFoodTime, ref food, ref newFirstHead);
                CheckIfOnFood(secondSnake, obstacles, foodGenerator, lastFoodTime, ref food, ref newSecondHead);

                if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                {
                    Console.SetCursorPosition(food.X, food.Y);
                    Console.Write(" ");
                    do
                    {
                        food = new Position(foodGenerator.Next(0, Console.WindowWidth - 1),
                        foodGenerator.Next(3, Console.WindowHeight - 1));
                    }
                    while ((firstSnake.Contains(food) && secondSnake.Contains(food)) || obstacles.Contains(food));
                    lastFoodTime = Environment.TickCount;

                }
                Console.SetCursorPosition(food.X, food.Y);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("+");

                // Slow the motion
                Thread.Sleep((int)snakeSpeed);

                // Update       
                int totalScoreOne = Math.Max(firstScore, 0);
                int totalScoreTwo = Math.Max(secondScore, 0);
                snakeSpeed -= 0.05;
                firstScore = firstSnake.Count - firstSnakeMinus;
                secondScore = secondSnake.Count - secondSnakeMinus;

                //Print ScoreBoard
                KeepScore(firstScore, secondScore);

            }
            ResetGame(winner, firstScore, secondScore, headSmash);
            if (playGame == false)
            {
                Console.WriteLine();

                Console.WriteLine("                     Thank you for playing! Enjoy the music...");
                EndGame();
                Console.WriteLine();
            }

            Console.Clear();
        }
    }

    private static bool Teleport(Queue<Position> hitSnake, ref Queue<Position> hittingSnake,
        List<Position> obstacles, Position[] snakeDirections, int snakeStart, int score,
        ref Position prevHead, ref Position newHead, ConsoleColor color)
    {
        if (hitSnake.Contains(newHead) ||
            obstacles.Contains(newHead) ||
            hittingSnake.Contains(newHead))
        {
            foreach (var elem in hittingSnake)
            {
                Console.SetCursorPosition(elem.X, elem.Y);
                Console.WriteLine(" ");
            }
            hittingSnake = GenerateSnake();
            InitialiseSnake(hittingSnake, score - 3, snakeStart, color);
            prevHead = hittingSnake.Last();
            newHead = NewSnakeHead(snakeDirections, 0, ref prevHead);
            return true;
        }
        return false;
    }

    private static void DrawScoreBoard()
    {
        for (int i = 0; i < Console.WindowWidth; i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(i, 2);
            Console.Write('_');
        }
    }
    private static void KeepScore(int firstPoints, int secondPoints)
    {
        Console.SetCursorPosition(2, 1);
        Console.Write("{0}'s Score: {1}   ", Player1Name, firstPoints);
        Console.Write("{0}'s Score: {1}", Player2Name, secondPoints);
    }

    struct Position
    {
        public int X, Y;

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public static void ConfigureConsole()
    {
        Console.CursorVisible = false;
        Console.BufferHeight = Console.WindowHeight;
    }

    static Queue<Position> GenerateSnake()
    {
        Queue<Position> snakeBody = new Queue<Position>();
        return snakeBody;
    }

    static List<Position> GenerateObstacle()
    {
        List<Position> obstacles = new List<Position>()
            {
                new Position (55, 3),
                new Position (15, 23),
                new Position (40, 19),
                new Position (69, 12),
                new Position (24, 9),              
                new Position (36, 15),
            };

        return obstacles;
    }

    static void InitialiseObstacles(List<Position> obstacles)
    {
        foreach (Position obstacle in obstacles)
        {
            Console.SetCursorPosition(obstacle.X, obstacle.Y);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("X");
        }
    }

    static Random FoodGenerator()
    {
        Random foodGenerator = new Random();
        return foodGenerator;
    }

    static double SetSpeed(double speed)
    {
        double sleepTime = speed;
        return sleepTime;
    }

    static Position[] SnakeDirections()
    {
        Position[] snakeDirections = new Position[]
        {
            new Position(1, 0), // Right
            new Position(0, 1), // Down
            new Position(-1, 0), // Left
            new Position(0, -1), // Up
        };
        return snakeDirections;
    }

    static Position Food(Random foodGenerator)
    {
        Position food = new Position(
        foodGenerator.Next(0, Console.WindowWidth - 1),
        foodGenerator.Next(3, Console.WindowHeight - 1));
        return food;
    }

    static Queue<Position> InitialiseSnake(Queue<Position> snakeBody, int elemNumber, int snakeNumber, ConsoleColor color)
    {
        for (int i = 0; i <= elemNumber; i++)
        {
            snakeBody.Enqueue(new Position(i, snakeNumber));
        }
        foreach (var item in snakeBody)
        {
            Console.SetCursorPosition(item.X, item.Y);
            Console.ForegroundColor = color;
            Console.Write("*");
        }
        Position snakeHead = snakeBody.Last();
        Console.SetCursorPosition(snakeHead.X, snakeHead.Y);
        Console.ForegroundColor = color;
        Console.Write("*");
        return snakeBody;
    }

    private static void WriteNewHead(Position prevHead, ConsoleColor color)
    {
        Console.SetCursorPosition(prevHead.X, prevHead.Y);
        Console.ForegroundColor = color;
        Console.Write("*");
    }

    private static Position AddNewSnakeElem(Queue<Position> snake, Position newHead, ConsoleColor color)
    {
        snake.Enqueue(newHead);
        Console.SetCursorPosition(newHead.X, newHead.Y);
        Console.ForegroundColor = color;
        Console.Write("@");
        return newHead;
    }

    private static void CheckIfOnFood(Queue<Position> snake, List<Position> obstacles, Random foodGenerator, int lastFoodTime, ref Position food, ref Position newHead)
    {
        if (newHead.X == food.X && newHead.Y == food.Y)
        {
            // Feed the snake (the snake is eating)
            do
            {
                food = new Position(foodGenerator.Next(0, Console.WindowWidth - 1),
                foodGenerator.Next(3, Console.WindowHeight - 1));
            }
            while (snake.Contains(food) || obstacles.Contains(food));
            lastFoodTime = Environment.TickCount;
            Console.SetCursorPosition(food.X, food.Y);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("+");
        }
        else
        {
            // Remove last snake element (the snake is moving)
            Position p = snake.Dequeue();
            Console.SetCursorPosition(p.X, p.Y);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" ");
        }
    }

    private static Position NewSnakeHead(Position[] snakeDirections, int currentDirection, ref Position prevFirstHead)
    {
        Position newFirstHead = new Position(
            prevFirstHead.X + snakeDirections[currentDirection].X,
            prevFirstHead.Y + snakeDirections[currentDirection].Y);
        return newFirstHead;
    }

    public static int WelcomeScreen()
    {
        Console.CursorVisible = true;
        //------Set Colors
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Clear();

        int i = -8;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write("--------------------------------------------------------------------");
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write("----------------------------MultiSnake------------------------------");
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write("--------------------------------------------------------------------");

        i++;
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 68));

        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 3) + "Player 1 Controls: Arrow keys;  Player 2 Controls: WASD" + new string('-', 10));

        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 3) + "Legend: The yellow plus is the Apple, while an X is an obstacle" + new string('-', 2));

        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 3) + "Tip: Press Spacebar to Pause the game" + new string('-', 28));

        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 3) + "Tip: Your score is equal to the length of your snake" + new string('-', 13));
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 3) + "Tip: Hitting an opponent/osbtacle makes you lose 2 elements" + new string('-', 6));
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write(new string('-', 3) + "Tip: Watch out for the walls or it's Game Over for you" + new string('-', 11));
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);

        Console.Write(new string('-', 3) + "Good Luck!" + new string('-', 55));
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write("--------------------------------------------------------------------");
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);

        Console.Write("Player 1: Please enter your name: ");
        Player1Name = Console.ReadLine();
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write("Player 2: Please enter your name: ");
        Player2Name = Console.ReadLine();
        while (true)
        {
            Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
            Console.Write("Select MultiSnake Mode: (1. Default mode; 2. Free Mode): ");
            try
            {
                GameMode = int.Parse(Console.ReadLine());
                if (GameMode < 1 || GameMode > 2)
                {
                    throw new ArgumentException();
                }
                break;
            }
            catch (FormatException)
            {
                Console.WriteLine("      Input can only be 1 or 2 !!!");
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("      Input can only be 1 or 2 !!!");
            }
            catch (OverflowException)
            {
                Console.WriteLine("      Input can only be 1 or 2 !!!");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("      Input can only be 1 or 2 !!!");
            }
            i--;
        }
        Console.WriteLine("                             ");
        Console.SetCursorPosition(Console.WindowWidth / 2 - 34, Console.WindowHeight / 2 + ++i);
        Console.Write("When you are ready, press any key to start the game: ");
        Console.ReadKey(true);
        Console.Clear();
        return GameMode;
    }

    public static int GameMode;
    public static string Player1Name;
    public static string Player2Name;
    public static bool playGame = true;
    public static ConsoleKey key = ConsoleKey.Spacebar;
    public static void ResetGame(int winner, int firstScore, int secondScore, bool headSmash)
    {
        if (key != ConsoleKey.Escape)
        {
            //Draw Game Over screen
            int i = -3;
            Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
            Console.Write("------------------------------------");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
            if (headSmash)
            {
                if (firstScore == secondScore)
                {
                    Console.Write("---The snakes smashed each other----");
                    Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
                    Console.Write("-Both dead & both with equal score!-");
                }
                else
                {
                    Console.Write("---The snakes smashed each other----");
                    Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
                    Console.Write("--Both dead, but Player {0} wins!---", winner);
                }
            }
            else
            {
                Console.Write("-----------Player {0} wins!-----------", winner);
            }
            Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
            Console.Write("---To play again press Enter--------");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
            Console.Write("---To Reset the game, press R-------");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
            Console.Write("---To quit press Escape-------------");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 19, Console.WindowHeight / 2 + ++i);
            Console.Write("------------------------------------");

            ExportScoreToTxt(winner, firstScore, secondScore);

            while (key != ConsoleKey.Enter && key != ConsoleKey.Escape)
            {
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter) { playGame = true; }
                if (key == ConsoleKey.Escape) { playGame = false; }
                if (key == ConsoleKey.R)
                {
                    WelcomeScreen();
                    playGame = true;
                }
            }
            key = 0;
        }
        else
        {
            playGame = false;
        }
    }

    private static void ExportScoreToTxt(int winner, int firstScore, int secondScore)
    {
        using (StreamWriter output = new StreamWriter("../../GameScoreHistory.txt", true))
        {
            output.WriteLine("Game ended on " + DateTime.Now);
            if (winner == 1)
            {
                output.WriteLine("Winner: " + Player1Name);
            }
            else if (winner == 2)
            {
                output.WriteLine("Winner: " + Player2Name);
            }

            output.WriteLine("{0} finished with {1} points", Player1Name, firstScore);
            output.WriteLine("{0} finished with {1} points", Player2Name, secondScore);
            output.WriteLine();

        }
    }

    public static void PauseGame()
    {
        Console.SetCursorPosition(69, 1);
        Console.Write("Game Paused");
        Console.ReadKey(true);
        key = ConsoleKey.Enter;
        Console.SetCursorPosition(69, 1);
        Console.Write("           ");
    }

    static void EndGame()
    {
        Console.Beep(659, 125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(523, 125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(784, 125);
        Thread.Sleep(375);
        Console.Beep(392, 125);
        Thread.Sleep(375);
        Console.Beep(523, 125);
        Thread.Sleep(250);
        Console.Beep(392, 125);
        Thread.Sleep(250);
        Console.Beep(330, 125);
        Thread.Sleep(250);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(494, 125);
        Thread.Sleep(125);
        Console.Beep(466, 125);
        Thread.Sleep(42);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(392, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(880, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(587, 125);
        Console.Beep(494, 125);
        Thread.Sleep(125);
        Console.Beep(523, 125);
        Thread.Sleep(250);
        Console.Beep(392, 125);
        Thread.Sleep(250);
        Console.Beep(330, 125);
        Thread.Sleep(250);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(494, 125);
        Thread.Sleep(125);
        Console.Beep(466, 125);
        Thread.Sleep(42);
        Console.Beep(440, 125);
        Thread.Sleep(125);
        Console.Beep(392, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(880, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(784, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(587, 125);
        Console.Beep(494, 125);
        Thread.Sleep(375);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(698, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(698, 125);
        Thread.Sleep(625);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(622, 125);
        Thread.Sleep(250);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(523, 125);
        Thread.Sleep(1125);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(698, 125);
        Thread.Sleep(125);
        Console.Beep(698, 125);
        Console.Beep(698, 125);
        Thread.Sleep(625);
        Console.Beep(784, 125);
        Console.Beep(740, 125);
        Console.Beep(698, 125);
        Thread.Sleep(42);
        Console.Beep(622, 125);
        Thread.Sleep(125);
        Console.Beep(659, 125);
        Thread.Sleep(167);
        Console.Beep(415, 125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Thread.Sleep(125);
        Console.Beep(440, 125);
        Console.Beep(523, 125);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(622, 125);
        Thread.Sleep(250);
        Console.Beep(587, 125);
        Thread.Sleep(250);
        Console.Beep(523, 125);
        Thread.Sleep(625);
    }
}