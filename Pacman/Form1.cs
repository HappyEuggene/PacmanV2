using System;
using System.Windows.Forms;

namespace Pacman
{
    public partial class Form1 : Form
    {
        public static GameBoard gameboard = new GameBoard();
        public static Food food = new Food();
        public static Pacman pacman = new Pacman();
        public static Ghost ghost = new Ghost();
        public static Player player = new Player();
        private static FormElements formelements = new FormElements();

        public Form1()
        {
            InitializeComponent();
            SetupGame();
        }

        public void SetupGame()
        {
            // Create Game Board
            gameboard.CreateBoardImage(this);

            // Create Board Matrix
            Tuple<int, int> PacmanStartCoordinates = gameboard.InitialiseBoardMatrix();

            // Create Player
            player.CreatePlayerScore(this);
            player.CreateLives(this);

            // Create Form Elements
            formelements.CreateFormScoreLabel(this);

            // Create Food
            food.CreateFoodImages(this);

            // Create Ghosts
            ghost.CreateGhostImage(this);

            // Create Pacman
            pacman.CreatePacmanImage(this, PacmanStartCoordinates.Item1, PacmanStartCoordinates.Item2);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Up: pacman.nextDirection = 1; pacman.MovePacman(1); break;
                case Keys.Right: pacman.nextDirection = 2; pacman.MovePacman(2); break;
                case Keys.Down: pacman.nextDirection = 3; pacman.MovePacman(3); break;
                case Keys.Left: pacman.nextDirection = 4; pacman.MovePacman(4); break;
            }
        }
	}
}
