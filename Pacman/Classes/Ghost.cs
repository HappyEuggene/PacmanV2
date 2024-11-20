using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Pacman
{
    public class Ghost
    {
        private const int GhostAmount = 4; // Загальна кількість привидів

        public int Ghosts = GhostAmount;
        private ImageList GhostImages = new ImageList(); // Список зображень привидів
        public PictureBox[] GhostImage = new PictureBox[GhostAmount]; // PictureBox для кожного привида
        public int[] State = new int[GhostAmount]; // Стан кожного привида
        private Timer timer = new Timer(); // Таймер для руху привидів
        private Timer killabletimer = new Timer(); // Таймер для привидів у стані killable
        private Timer statetimer = new Timer(); // Таймер для скидання стану привидів
        private Timer hometimer = new Timer(); // Таймер для повернення привидів додому
        public int[] xCoordinate = new int[GhostAmount]; // Координати X для кожного привида
        public int[] yCoordinate = new int[GhostAmount]; // Координати Y для кожного привида
        private int[] xStart = new int[GhostAmount]; // Початкові координати X
        private int[] yStart = new int[GhostAmount]; // Початкові координати Y
        public int[] Direction = new int[GhostAmount]; // Напрямок для кожного привида
        private Random ran = new Random(); // Генератор випадкових чисел
        private bool GhostOn = false; // Для анімації привидів

        // Перерахування для алгоритмів пошуку
        private enum SearchAlgorithm { Random, BFS, DFS, GreedyBestFirst }
        private SearchAlgorithm[] ghostAlgorithms = new SearchAlgorithm[GhostAmount];

        public Ghost()
        {
            // Додавання всіх зображень привидів до GhostImages
            // Переконайтеся, що всі необхідні зображення додані до ресурсів проекту
            GhostImages.Images.Add(Properties.Resources.Ghost_0_1);
            GhostImages.Images.Add(Properties.Resources.Ghost_0_2);
            GhostImages.Images.Add(Properties.Resources.Ghost_0_3);
            GhostImages.Images.Add(Properties.Resources.Ghost_0_4);

            GhostImages.Images.Add(Properties.Resources.Ghost_1_1);
            GhostImages.Images.Add(Properties.Resources.Ghost_1_2);
            GhostImages.Images.Add(Properties.Resources.Ghost_1_3);
            GhostImages.Images.Add(Properties.Resources.Ghost_1_4);

            GhostImages.Images.Add(Properties.Resources.Ghost_2_1);
            GhostImages.Images.Add(Properties.Resources.Ghost_2_2);
            GhostImages.Images.Add(Properties.Resources.Ghost_2_3);
            GhostImages.Images.Add(Properties.Resources.Ghost_2_4);

            GhostImages.Images.Add(Properties.Resources.Ghost_3_1);
            GhostImages.Images.Add(Properties.Resources.Ghost_3_2);
            GhostImages.Images.Add(Properties.Resources.Ghost_3_3);
            GhostImages.Images.Add(Properties.Resources.Ghost_3_4);

            GhostImages.Images.Add(Properties.Resources.Ghost_4); // Зображення killable 1
            GhostImages.Images.Add(Properties.Resources.Ghost_5); // Зображення killable 2
            GhostImages.Images.Add(Properties.Resources.eyes);    // Зображення очей (мертвий привид)

            GhostImages.ImageSize = new Size(27, 28); // Встановлення розміру зображень привидів

            // Ініціалізація всіх таймерів
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Tick += new EventHandler(timer_Tick);

            killabletimer.Interval = 200;
            killabletimer.Enabled = false;
            killabletimer.Tick += new EventHandler(killabletimer_Tick);

            statetimer.Interval = 10000;
            statetimer.Enabled = false;
            statetimer.Tick += new EventHandler(statetimer_Tick);

            hometimer.Interval = 5;
            hometimer.Enabled = false;
            hometimer.Tick += new EventHandler(hometimer_Tick);

            // Призначення алгоритмів привидам
            AssignAlgorithms();
        }

        private void AssignAlgorithms()
        {
            // Призначаємо різні алгоритми привидам
            ghostAlgorithms[0] = SearchAlgorithm.Random; // Привид 0 використовує випадковий метод
            ghostAlgorithms[1] = SearchAlgorithm.DFS;
            ghostAlgorithms[2] = SearchAlgorithm.BFS;
            ghostAlgorithms[3] = SearchAlgorithm.GreedyBestFirst;
        }

        public void CreateGhostImage(Form formInstance)
        {
            // Створення PictureBox та додавання їх до форми для кожного привида
            for (int x = 0; x < Ghosts; x++)
            {
                GhostImage[x] = new PictureBox();
                GhostImage[x].Name = "GhostImage" + x.ToString();
                GhostImage[x].SizeMode = PictureBoxSizeMode.AutoSize;
                formInstance.Controls.Add(GhostImage[x]);
                GhostImage[x].BringToFront();
            }
            Set_Ghosts(); // Встановлення початкових позицій привидів
            ResetGhosts(); // Скидання стану та місцезнаходження привидів
        }

        public void Set_Ghosts()
        {
            // Пошук початкових позицій для привидів на ігровому полі
            int Amount = -1;
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 27; x++)
                {
                    if (Form1.gameboard.Matrix[y, x] == 15)
                    {
                        Amount++;
                        if (Amount < GhostAmount)
                        {
                            xStart[Amount] = x;
                            yStart[Amount] = y;
                            System.Diagnostics.Debug.WriteLine($"Ghost {Amount} starting at x={xStart[Amount]}, y={yStart[Amount]}");
                        }
                    }
                }
            }
            if (Amount + 1 < GhostAmount)
            {
                System.Diagnostics.Debug.WriteLine("Not enough starting positions for all ghosts.");
                // Можна встановити дефолтні позиції або вивести повідомлення про помилку
            }
        }

        public void ResetGhosts()
        {
            // Скидання стану та місцезнаходження привидів
            for (int x = 0; x < GhostAmount; x++)
            {
                xCoordinate[x] = xStart[x];
                yCoordinate[x] = yStart[x];
                GhostImage[x].Location = new Point(xStart[x] * 16 - 3, yStart[x] * 16 + 43);
                GhostImage[x].Image = GhostImages.Images[x * 4];
                Direction[x] = 0; // Починаємо без напрямку
                State[x] = 0;

                System.Diagnostics.Debug.WriteLine($"Ghost {x} reset: x = {xCoordinate[x]}, y = {yCoordinate[x]}, Direction = {Direction[x]}, State = {State[x]}");
            }
        }

        private void statetimer_Tick(object sender, EventArgs e)
        {
            // Скидання стану всіх привидів, щоб вони не були killable
            for (int x = 0; x < GhostAmount; x++)
            {
                State[x] = 0;
            }
            statetimer.Enabled = false;
        }

        private void hometimer_Tick(object sender, EventArgs e)
        {
            // Повернення привидів до початкових позицій
            for (int x = 0; x < GhostAmount; x++)
            {
                if (State[x] == 2)
                {
                    int xpos = xStart[x] * 16 - 3;
                    int ypos = yStart[x] * 16 + 43;
                    if (GhostImage[x].Left > xpos) { GhostImage[x].Left--; }
                    if (GhostImage[x].Left < xpos) { GhostImage[x].Left++; }
                    if (GhostImage[x].Top > ypos) { GhostImage[x].Top--; }
                    if (GhostImage[x].Top < ypos) { GhostImage[x].Top++; }
                    if (GhostImage[x].Top == ypos && GhostImage[x].Left == xpos)
                    {
                        State[x] = 0;
                        xCoordinate[x] = xStart[x];
                        yCoordinate[x] = yStart[x];
                        GhostImage[x].Left = xStart[x] * 16 - 3;
                        GhostImage[x].Top = yStart[x] * 16 + 43;
                    }
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // Рух усіх привидів
            for (int x = 0; x < Ghosts; x++)
            {
                if (State[x] > 0) { continue; } // Якщо привид у спеціальному стані, пропускаємо його рух
                MoveGhosts(x); // Рух привида за індексом
            }
            GhostOn = !GhostOn; // Зміна анімації привидів
            CheckForPacman(); // Перевірка зіткнення з Пакманом
        }

        private void killabletimer_Tick(object sender, EventArgs e)
        {
            // Рух привидів у стані killable
            for (int x = 0; x < Ghosts; x++)
            {
                if (State[x] != 1) { continue; } // Якщо привид не у режимі killable, пропускаємо його рух
                MoveGhosts(x); // Рух привида за індексом
            }
        }

        private void MoveGhosts(int x)
        {
            // Логування поточних координат та стану привида
            System.Diagnostics.Debug.WriteLine($"Ghost {x}: x = {xCoordinate[x]}, y = {yCoordinate[x]}, Direction = {Direction[x]}, State = {State[x]}");

            try
            {
                if (x == 0)
                {
                    // Привид 0 рухається за вашим кодом
                    MoveGhost0(x);
                }
                else
                {
                    // Інші привиди використовують призначені алгоритми
                    Point nextMove = GetNextMove(x);

                    if (nextMove.X != xCoordinate[x] || nextMove.Y != yCoordinate[x])
                    {
                        // Визначаємо напрямок на основі наступного кроку
                        if (nextMove.Y < yCoordinate[x]) { Direction[x] = 1; } // Вгору
                        else if (nextMove.X > xCoordinate[x]) { Direction[x] = 2; } // Вправо
                        else if (nextMove.Y > yCoordinate[x]) { Direction[x] = 3; } // Вниз
                        else if (nextMove.X < xCoordinate[x]) { Direction[x] = 4; } // Вліво
                    }
                    else
                    {
                        // Якщо немає руху, призначаємо випадковий напрямок
                        Direction[x] = ran.Next(1, 5); // Випадковий напрямок від 1 до 4
                    }

                    bool CanMove = check_direction(Direction[x], x); // Перевіряємо, чи можна рухатися в цьому напрямку

                    if (!CanMove)
                    {
                        Change_Direction(Direction[x], x); // Змінюємо напрямок, якщо не можна рухатися
                    }

                    if (CanMove)
                    {
                        switch (Direction[x])
                        {
                            case 1: GhostImage[x].Top -= 16; yCoordinate[x]--; break;
                            case 2: GhostImage[x].Left += 16; xCoordinate[x]++; break;
                            case 3: GhostImage[x].Top += 16; yCoordinate[x]++; break;
                            case 4: GhostImage[x].Left -= 16; xCoordinate[x]--; break;
                        }
                        switch (State[x])
                        {
                            case 0:
                                GhostImage[x].Image = GhostImages.Images[x * 4 + (Direction[x] - 1)];
                                break; // Встановлення зображення привида залежно від напрямку
                            case 1: // Якщо привид у стані killable, анімуємо блимаючий стан
                                if (GhostOn)
                                {
                                    GhostImage[x].Image = GhostImages.Images[16];
                                }
                                else
                                {
                                    GhostImage[x].Image = GhostImages.Images[17];
                                }
                                break;
                            case 2:
                                GhostImage[x].Image = Properties.Resources.eyes;
                                break; // Якщо привид мертвий, показуємо очі
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                System.Diagnostics.Debug.WriteLine($"IndexOutOfRangeException in MoveGhosts: {ex.Message}");
                // Можливо, скинути координати привида або виконати інші дії
            }
        }

        private void MoveGhost0(int x)
        {
            // Ваш код для руху привида 0
            if (Direction[x] == 0)
            {
                if (ran.Next(0, 5) == 3) { Direction[x] = 1; } // Шанс почати рух
            }
            else
            {
                bool CanMove = false;
                Other_Direction(Direction[x], x); // Перевірка, чи можна рухатися в іншому напрямку

                while (!CanMove)
                {
                    CanMove = check_direction(Direction[x], x); // Перевірка, чи можна рухатися в поточному напрямку
                    if (!CanMove) { Change_Direction(Direction[x], x); } // Якщо не можна, змінюємо напрямок
                }

                if (CanMove)
                {
                    switch (Direction[x])
                    {
                        case 1: GhostImage[x].Top -= 16; yCoordinate[x]--; break;
                        case 2: GhostImage[x].Left += 16; xCoordinate[x]++; break;
                        case 3: GhostImage[x].Top += 16; yCoordinate[x]++; break;
                        case 4: GhostImage[x].Left -= 16; xCoordinate[x]--; break;
                    }
                    switch (State[x])
                    {
                        case 0: GhostImage[x].Image = GhostImages.Images[x * 4 + (Direction[x] - 1)]; break; // Встановлення зображення
                        case 1:
                            if (GhostOn) { GhostImage[x].Image = GhostImages.Images[17]; } else { GhostImage[x].Image = GhostImages.Images[16]; };
                            break;
                        case 2: GhostImage[x].Image = GhostImages.Images[18]; break; // Якщо привид мертвий, показуємо очі
                    }
                }
            }
        }

        private bool check_direction(int direction, int ghost)
        {
            // Перевірка, чи можна рухатися у вказаному напрямку
            switch (direction)
            {
                case 1: return IsWalkable(xCoordinate[ghost], yCoordinate[ghost] - 1);
                case 2: return IsWalkable(xCoordinate[ghost] + 1, yCoordinate[ghost]);
                case 3: return IsWalkable(xCoordinate[ghost], yCoordinate[ghost] + 1);
                case 4: return IsWalkable(xCoordinate[ghost] - 1, yCoordinate[ghost]);
                default: return false;
            }
        }

        private bool IsWalkable(int x, int y)
        {
            // Перевірка, чи можна рухатися на вказану клітинку
            if (x < 0)
            {
                return true; // Дозволяємо перехід через край карти
            }
            if (x > 27)
            {
                return true; // Дозволяємо перехід через край карти
            }
            if (y < 0 || y >= Form1.gameboard.Matrix.GetLength(0) || x < 0 || x >= Form1.gameboard.Matrix.GetLength(1))
            {
                return false;
            }

            int cellValue = Form1.gameboard.Matrix[y, x];
            return (cellValue < 4 || cellValue > 10);
        }

        private void Change_Direction(int direction, int ghost)
        {
            // Зміна напрямку привида, якщо не можна рухатися в поточному напрямку
            int which = ran.Next(0, 2);
            switch (direction)
            {
                case 1:
                case 3:
                    if (which == 1) { Direction[ghost] = 2; } else { Direction[ghost] = 4; };
                    break;
                case 2:
                case 4:
                    if (which == 1) { Direction[ghost] = 1; } else { Direction[ghost] = 3; };
                    break;
            }
            System.Diagnostics.Debug.WriteLine($"Ghost {ghost} changed direction to {Direction[ghost]} in Change_Direction");
        }

        private void Other_Direction(int direction, int ghost)
        {
            // Перевірка, чи можна рухатися в іншому напрямку
            if (Form1.gameboard.Matrix[yCoordinate[ghost], xCoordinate[ghost]] < 4)
            {
                bool[] directions = new bool[5];
                int x = xCoordinate[ghost];
                int y = yCoordinate[ghost];
                switch (direction)
                {
                    case 1:
                    case 3:
                        directions[2] = IsWalkable(x + 1, y);
                        directions[4] = IsWalkable(x - 1, y);
                        break;
                    case 2:
                    case 4:
                        directions[1] = IsWalkable(x, y - 1);
                        directions[3] = IsWalkable(x, y + 1);
                        break;
                }
                int which = ran.Next(1, 5);
                if (directions[which])
                {
                    Direction[ghost] = which;
                    System.Diagnostics.Debug.WriteLine($"Ghost {ghost} changed direction to {Direction[ghost]} in Other_Direction");
                }
            }
        }

        private Point GetNextMove(int ghostIndex)
        {
            // Отримуємо наступний крок для привида на основі його алгоритму
            switch (ghostAlgorithms[ghostIndex])
            {
                case SearchAlgorithm.Random:
                    return GetRandomValidMove(new Point(xCoordinate[ghostIndex], yCoordinate[ghostIndex]));
                case SearchAlgorithm.BFS:
                    return BFS(ghostIndex);
                case SearchAlgorithm.DFS:
                    return DFS(ghostIndex);
                case SearchAlgorithm.GreedyBestFirst:
                    return GreedyBestFirstSearch(ghostIndex);
                default:
                    return new Point(xCoordinate[ghostIndex], yCoordinate[ghostIndex]); // Без зміни
            }
        }

        // Реалізації методів BFS, DFS та GreedyBestFirstSearch

        private Point BFS(int ghostIndex)
        {
            Queue<Point> queue = new Queue<Point>();
            bool[,] visited = new bool[Form1.gameboard.Matrix.GetLength(0), Form1.gameboard.Matrix.GetLength(1)];
            Point start = new Point(xCoordinate[ghostIndex], yCoordinate[ghostIndex]);
            Point target = new Point(Form1.pacman.xCoordinate, Form1.pacman.yCoordinate);
            queue.Enqueue(start);
            visited[start.Y, start.X] = true;

            Dictionary<Point, Point> parent = new Dictionary<Point, Point>();
            bool pathFound = false;

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                if (current.Equals(target))
                {
                    pathFound = true;
                    break;
                }

                foreach (Point neighbor in GetNeighbors(current))
                {
                    if (IsWalkable(neighbor.X, neighbor.Y) && !visited[neighbor.Y, neighbor.X])
                    {
                        queue.Enqueue(neighbor);
                        visited[neighbor.Y, neighbor.X] = true;
                        parent[neighbor] = current;
                    }
                }
            }

            if (pathFound)
            {
                // Відновлення шляху
                List<Point> path = new List<Point>();
                Point step = target;
                while (!step.Equals(start))
                {
                    path.Add(step);
                    if (parent.ContainsKey(step))
                    {
                        step = parent[step];
                    }
                    else
                    {
                        // Не вдалося відновити шлях
                        System.Diagnostics.Debug.WriteLine("Failed to reconstruct path in BFS.");
                        return GetRandomValidMove(start);
                    }
                }
                path.Reverse();
                return path[0]; // Повертаємо наступний крок
            }
            else
            {
                // Шлях не знайдено
                System.Diagnostics.Debug.WriteLine("Path not found in BFS.");
                return GetRandomValidMove(start);
            }
        }

        private List<Point> GetNeighborsWithVerticalPriority(Point p)
        {
            List<Point> neighbors = new List<Point>();
            int maxY = Form1.gameboard.Matrix.GetLength(0) - 1;
            int maxX = Form1.gameboard.Matrix.GetLength(1) - 1;

            // Додаємо сусідів у порядку: Вліво, Вправо, Вгору, Вниз
            if (p.X > 0)
                neighbors.Add(new Point(p.X - 1, p.Y)); // Вліво
            if (p.X < maxX)
                neighbors.Add(new Point(p.X + 1, p.Y)); // Вправо
            if (p.Y > 0)
                neighbors.Add(new Point(p.X, p.Y - 1)); // Вгору
            if (p.Y < maxY)
                neighbors.Add(new Point(p.X, p.Y + 1)); // Вниз

            return neighbors;
        }

        private List<Point> GetNeighborsOrderedByHeuristic(Point current, Point target)
        {
            List<Point> neighbors = new List<Point>();
            int maxY = Form1.gameboard.Matrix.GetLength(0) - 1;
            int maxX = Form1.gameboard.Matrix.GetLength(1) - 1;

            // Додаємо сусідів, якщо вони в межах поля
            if (current.Y > 0)
                neighbors.Add(new Point(current.X, current.Y - 1)); // Вгору
            if (current.X < maxX)
                neighbors.Add(new Point(current.X + 1, current.Y)); // Вправо
            if (current.Y < maxY)
                neighbors.Add(new Point(current.X, current.Y + 1)); // Вниз
            if (current.X > 0)
                neighbors.Add(new Point(current.X - 1, current.Y)); // Вліво

            // Сортуємо сусідів за евристичною відстанню до цілі
            neighbors.Sort((a, b) => Heuristic(a, target).CompareTo(Heuristic(b, target)));

            return neighbors;
        }


        private Point DFS(int ghostIndex)
        {
            Stack<Point> stack = new Stack<Point>();
            bool[,] visited = new bool[Form1.gameboard.Matrix.GetLength(0), Form1.gameboard.Matrix.GetLength(1)];
            Point start = new Point(xCoordinate[ghostIndex], yCoordinate[ghostIndex]);
            Point target = new Point(Form1.pacman.xCoordinate, Form1.pacman.yCoordinate);
            stack.Push(start);
            visited[start.Y, start.X] = true;

            Dictionary<Point, Point> parent = new Dictionary<Point, Point>();

            while (stack.Count > 0)
            {
                Point current = stack.Pop();
                System.Diagnostics.Debug.WriteLine($"DFS visiting x={current.X}, y={current.Y}");

                if (current.Equals(target))
                {
                    // Відновлення шляху
                    List<Point> path = new List<Point>();
                    Point step = current;
                    while (!step.Equals(start))
                    {
                        path.Add(step);
                        step = parent[step];
                    }
                    path.Reverse();
                    return path[0]; // Повертаємо наступний крок
                }

                foreach (Point neighbor in GetNeighborsOrderedByHeuristic(current, target))
                {
                    if (IsWalkable(neighbor.X, neighbor.Y) && !visited[neighbor.Y, neighbor.X])
                    {
                        stack.Push(neighbor);
                        visited[neighbor.Y, neighbor.X] = true;
                        parent[neighbor] = current;
                    }
                }
            }

            // Шлях не знайдено
            System.Diagnostics.Debug.WriteLine("Path not found in DFS.");
            return GetRandomValidMove(start);
        }



        private Point GreedyBestFirstSearch(int ghostIndex)
        {
            SimplePriorityQueue<Point> openSet = new SimplePriorityQueue<Point>();
            bool[,] closedSet = new bool[Form1.gameboard.Matrix.GetLength(0), Form1.gameboard.Matrix.GetLength(1)];
            Point start = new Point(xCoordinate[ghostIndex], yCoordinate[ghostIndex]);
            Point target = new Point(Form1.pacman.xCoordinate, Form1.pacman.yCoordinate);
            openSet.Enqueue(start, Heuristic(start, target));

            Dictionary<Point, Point> parent = new Dictionary<Point, Point>();

            while (!openSet.IsEmpty())
            {
                Point current = openSet.Dequeue();

                if (current.Equals(target))
                {
                    // Відновлення шляху
                    List<Point> path = new List<Point>();
                    Point step = target;
                    while (!step.Equals(start))
                    {
                        path.Add(step);
                        if (parent.ContainsKey(step))
                        {
                            step = parent[step];
                        }
                        else
                        {
                            // Не вдалося відновити шлях
                            System.Diagnostics.Debug.WriteLine("Failed to reconstruct path in GreedyBestFirstSearch.");
                            return GetRandomValidMove(start);
                        }
                    }
                    path.Reverse();
                    return path[0]; // Повертаємо наступний крок
                }

                closedSet[current.Y, current.X] = true;

                foreach (Point neighbor in GetNeighbors(current))
                {
                    if (closedSet[neighbor.Y, neighbor.X] || !IsWalkable(neighbor.X, neighbor.Y))
                        continue;

                    if (!parent.ContainsKey(neighbor))
                    {
                        parent[neighbor] = current;
                        double priority = Heuristic(neighbor, target);
                        openSet.Enqueue(neighbor, priority);
                    }
                }
            }

            // Шлях не знайдено
            System.Diagnostics.Debug.WriteLine("Path not found in GreedyBestFirstSearch.");
            return GetRandomValidMove(start);
        }

        private double Heuristic(Point a, Point b)
        {
            // Манхеттенська відстань
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private List<Point> GetNeighbors(Point p)
        {
            List<Point> neighbors = new List<Point>();
            int maxY = Form1.gameboard.Matrix.GetLength(0) - 1;
            int maxX = Form1.gameboard.Matrix.GetLength(1) - 1;

            if (p.Y > 0)
                neighbors.Add(new Point(p.X, p.Y - 1));
            if (p.X < maxX)
                neighbors.Add(new Point(p.X + 1, p.Y));
            if (p.Y < maxY)
                neighbors.Add(new Point(p.X, p.Y + 1));
            if (p.X > 0)
                neighbors.Add(new Point(p.X - 1, p.Y));
            return neighbors;
        }

        private Point GetRandomValidMove(Point current)
        {
            List<Point> validNeighbors = new List<Point>();

            foreach (Point neighbor in GetNeighbors(current))
            {
                if (IsWalkable(neighbor.X, neighbor.Y))
                {
                    validNeighbors.Add(neighbor);
                }
            }

            if (validNeighbors.Count > 0)
            {
                int index = ran.Next(validNeighbors.Count);
                Point randomMove = validNeighbors[index];
                System.Diagnostics.Debug.WriteLine($"Ghost is moving randomly to x={randomMove.X}, y={randomMove.Y}");
                return randomMove;
            }
            else
            {
                // Немає доступних ходів, залишаємося на місці
                return current;
            }
        }

        public void ChangeGhostState()
        {
            // Зміна стану всіх привидів на killable
            for (int x = 0; x < GhostAmount; x++)
            {
                if (State[x] == 0)
                {
                    State[x] = 1;
                    GhostImage[x].Image = GhostImages.Images[16];
                    System.Diagnostics.Debug.WriteLine($"Ghost {x} is now killable.");
                }
            }
            killabletimer.Stop();
            killabletimer.Enabled = true;
            killabletimer.Start();
            statetimer.Stop();
            statetimer.Enabled = true;
            statetimer.Start();
        }

        public void CheckForPacman()
        {
            // Перевірка, чи якийсь привид зіткнувся з Пакманом
            for (int x = 0; x < Ghosts; x++)
            {
                if (xCoordinate[x] == Form1.pacman.xCoordinate && yCoordinate[x] == Form1.pacman.yCoordinate)
                {
                    switch (State[x])
                    {
                        case 0:
                            Form1.player.LoseLife();
                            System.Diagnostics.Debug.WriteLine($"Ghost {x} collided with Pacman. Pacman loses a life.");
                            break; // Якщо привид не killable, Пакман втрачає життя
                        case 1: // Якщо привид killable, він повертається додому
                            State[x] = 2;
                            hometimer.Enabled = true;
                            GhostImage[x].Image = Properties.Resources.eyes;
                            Form1.player.UpdateScore(300);
                            System.Diagnostics.Debug.WriteLine($"Ghost {x} was eaten by Pacman. Score increased by 300.");
                            break;
                    }
                }
            }
        }
    }
}
