using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TicTac
{
    public partial class MainWindow : Window
    {
        private string[] board = new string[9];
        private bool isPlayerTurn = true;
        private Random rand = new Random();

        // бот
        private enum Difficulty { Easy, Medium, Hard }
        private Difficulty currentDifficulty = Difficulty.Easy;

        public MainWindow()
        {
            InitializeComponent();
            ResetBoard();
        }

        private void ResetBoard()
        {
            for (int i = 0; i < 9; i++)
            {
                board[i] = "";
                var btn = (Button)GameGrid.Children[i];
                btn.Content = "";
                btn.IsEnabled = true;
            }
            StatusText.Text = "ваш ход!";
            isPlayerTurn = true;
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)DifficultyComboBox.SelectedItem;
            string tag = selectedItem.Tag.ToString();

            switch (tag)
            {
                case "Easy":
                    currentDifficulty = Difficulty.Easy;
                    break;
                case "Medium":
                    currentDifficulty = Difficulty.Medium;
                    break;
                case "Hard":
                    currentDifficulty = Difficulty.Hard;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayerTurn) return;

            Button btn = (Button)sender;
            int index = GameGrid.Children.IndexOf(btn);

            if (board[index] == "")
            {
                MakeMove(index, "X");
                if (CheckWin("X"))
                {
                    StatusText.Text = "вы победили!";
                    EndGame();
                    return;
                }
                if (IsDraw())
                {
                    StatusText.Text = "ничья!";
                    EndGame();
                    return;
                }

                isPlayerTurn = false;
                BotMove();
            }
        }

        // сложности
        private int EasyBot() // лёгкий бот. рандомные ходы
        {
            var emptyIndices = board.Select((val, idx) => new { val, idx })
                                    .Where(x => x.val == "")
                                    .Select(x => x.idx).ToList();
            return emptyIndices[rand.Next(emptyIndices.Count)];
        }

        private int? MediumBot() // средний бот. пытается искать выигрышный ход. если подходящего нет - null
        {
            int? move = null;
            string[] symbols = { "O", "X" }; 

            foreach (var symbol in symbols)
            {
                var emptyIndices = board.Select((val, idx) => new { val, idx })
                                        .Where(x => x.val == "")
                                        .Select(x => x.idx).ToList();
                // выиграть, затем блокировать
                foreach (var idx in emptyIndices)
                {
                    board[idx] = symbol;
                    if (CheckWin(symbol))
                    {
                        board[idx] = "";
                        return idx;
                    }
                    board[idx] = "";
                }
            }

            return null;
        }

        // для этого бота загуглил и заюзал min-max алгоритм. ищёт лучший ход и пытается вывести на ничью или победить
        private int HardBot()
        {
            int bestScore = int.MinValue;
            int move = -1;

            for (int i = 0; i < 9; i++)
            {
                if (board[i] == "")
                {
                    board[i] = "O";
                    int score = Minimax(board, false);
                    board[i] = "";

                    if (score > bestScore)
                    {
                        bestScore = score;
                        move = i;
                    }
                }
            }
            return move;
        }

        private int Minimax(string[] currentBoard, bool isMaximizing)
        {
            if (CheckWin("O"))
                return 10; // если выиграл бот
            if (CheckWin("X"))
                return -10; // если выиграл игрок
            if (currentBoard.All(s => s != ""))
                return 0;

            // бот ходит
            if (isMaximizing)
            {
                int bestScore = int.MinValue; // наименьшее зн.
                for (int i = 0; i < 9; i++)
                {
                    if (currentBoard[i] == "")
                    {
                        currentBoard[i] = "O"; // предположительный ход
                        int score = Minimax(currentBoard, false); // для хода игрока
                        currentBoard[i] = "";
                        // обновляем результат
                        if (score > bestScore)
                            bestScore = score;
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < 9; i++)
                {
                    if (currentBoard[i] == "")
                    {
                        currentBoard[i] = "X"; // предположительный ход игрока
                        int score = Minimax(currentBoard, true); // для хода бота
                        currentBoard[i] = "";
                        // обновляем
                        if (score < bestScore)
                            bestScore = score;
                    }
                }
                return bestScore;
            }
        }

        private void BotMove()
        {
            int moveIndex;

            switch (currentDifficulty)
            {
                case Difficulty.Easy:
                    moveIndex = EasyBot();
                    break;
                case Difficulty.Medium:
                    moveIndex = MediumBot() ?? EasyBot();
                    break;
                case Difficulty.Hard:
                    moveIndex = HardBot(); // можно заменить на EasyBot() или MediumBot() ?? EasyBot() если прога долго запускается
                    break;
                default:
                    moveIndex = EasyBot();
                    break;
            }

            MakeMove(moveIndex, "O");

            if (CheckWin("O"))
            {
                StatusText.Text = "бот победил!";
                EndGame();
                return;
            }
            if (IsDraw())
            {
                StatusText.Text = "ничья!";
                EndGame();
                return;
            }

            isPlayerTurn = true;
            StatusText.Text = "ваш ход!";
        }

        private void MakeMove(int index, string symbol)
        {
            board[index] = symbol;
            var btn = (Button)GameGrid.Children[index];
            btn.Content = symbol;
            btn.IsEnabled = false;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBoard();
        }

        private bool CheckWin(string symbol)
        {
            int[][] wins = new int[][]
            {
                new int[]{0,1,2},
                new int[]{3,4,5},
                new int[]{6,7,8},
                new int[]{0,3,6},
                new int[]{1,4,7},
                new int[]{2,5,8},
                new int[]{0,4,8},
                new int[]{2,4,6}
            };

            return wins.Any(w => w.All(i => board[i] == symbol));
        }

        private bool IsDraw()
        {
            return board.All(s => s != "");
        }

        private void EndGame()
        {
            foreach (Button btn in GameGrid.Children)
            {
                btn.IsEnabled = false;
            }
        }
    }
}