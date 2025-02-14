using laba1;
using System.Windows;
using System.Windows.Media;

public class CheckerGame
{
    public Checker[,] Board { get; private set; } = new Checker[8, 8];
    public CheckerColor CurrentTurn { get; set; } = CheckerColor.White;
    private bool mustContinueJump = false;
    public event Action<CheckerColor> TurnChanged;
    private readonly MediaPlayer mediaPlayer;


    public CheckerGame()
    {
        InitializeBoard();
        mediaPlayer = new MediaPlayer();
    }

    public void PlaySound(string soundPath)
    {
        mediaPlayer.Open(new Uri(soundPath, UriKind.Relative)); 
        mediaPlayer.Play();
    }

    private void InitializeBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    if (row < 3)
                    {
                        Board[row, col] = new Checker(CheckerColor.White, row, col);
                    }
                    else if (row > 4)
                    {
                        Board[row, col] = new Checker(CheckerColor.Black, row, col);
                    }
                }
            }
        }
    }

    public void Restart()
    {
        Board = new Checker[8, 8];

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1) 
                {
                    if (row < 3)
                    {
                        Board[row, col] = new Checker(CheckerColor.White, row, col);
                    }
                    else if (row > 4)
                    {
                        Board[row, col] = new Checker(CheckerColor.Black, row, col);
                    }
                }
            }
        }

        CurrentTurn = CheckerColor.White;
    }


    private void PerformMove(Checker checker, int fromRow, int fromCol, int toRow, int toCol)
    {
        Board[toRow, toCol] = checker;
        Board[fromRow, fromCol] = null;
        checker.Row = toRow;
        checker.Col = toCol;

        PromoteToKing(checker);
    }

    private void PromoteToKing(Checker checker)
    {
        if ((checker.Color == CheckerColor.Black && checker.Row == 0) ||
            (checker.Color == CheckerColor.White && checker.Row == 7))
        {
            checker.Type = CheckerType.King;
            PlaySound("dungeon-master.mp3");
        }
    }
    public bool MoveChecker(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (Board[fromRow, fromCol] == null || Board[toRow, toCol] != null)
        {
            return false;
        }

        Checker checker = Board[fromRow, fromCol];

        if (checker.Color != CurrentTurn)
        {
            return false;
        }

        int rowDiff = toRow - fromRow;
        int colDiff = Math.Abs(toCol - fromCol);
        bool isCapture = Math.Abs(rowDiff) == 2 && colDiff == 2;
        if (checker.Type == CheckerType.Regular)
        {
            if (isCapture)
            {

                int midRow = (fromRow + toRow) / 2;
                int midCol = (fromCol + toCol) / 2;

                if (Board[midRow, midCol] == null || Board[midRow, midCol].Color == checker.Color)
                {
                    return false;
                }

                if (!mustContinueJump)
                {
                    if ((CurrentTurn == CheckerColor.White && rowDiff < 0) ||
                        (CurrentTurn == CheckerColor.Black && rowDiff > 0))
                    {
                        return false;
                    }
                }

                Board[midRow, midCol] = null;
                PerformMove(checker, fromRow, fromCol, toRow, toCol);

                if (CanJumpAgain(toRow, toCol))
                {
                    mustContinueJump = true;
                    PlaySound("spank.mp3");
                    return true;

                    
                }
                else
                {
                    mustContinueJump = false;
                    PlaySound("spank.mp3");
                    // MessageBox.Show("ого");
                    SwitchTurn();
                }

                CheckForWin();
                return true;
            }
            else if (Math.Abs(rowDiff) == 1 && colDiff == 1 && !mustContinueJump)
            {
                if (!mustContinueJump)
                {
                    if ((CurrentTurn == CheckerColor.White && rowDiff < 0) ||
                        (CurrentTurn == CheckerColor.Black && rowDiff > 0))
                    {
                        return false;
                    }
                }
                PerformMove(checker, fromRow, fromCol, toRow, toCol);
                mustContinueJump = false;
                SwitchTurn();
                CheckForWin();
                return true;
            }

        }

        if (checker.Type == CheckerType.King)
        {
            if (Math.Abs(toRow - fromRow) != Math.Abs(toCol - fromCol))
                return false;

            int rowDirection = (toRow - fromRow) > 0 ? 1 : -1;
            int colDirection = (toCol - fromCol) > 0 ? 1 : -1;

            int currentRow = fromRow;
            int currentCol = fromCol;
            int capturedRow = -1, capturedCol = -1;
            bool foundCapture = false;

            while (currentRow != toRow || currentCol != toCol)
            {
                currentRow += rowDirection;
                currentCol += colDirection;

                if (currentRow < 0 || currentRow >= Board.GetLength(0) ||
                    currentCol < 0 || currentCol >= Board.GetLength(1))
                {
                    return false;
                }

                if (Board[currentRow, currentCol] != null)
                {
                    if (Board[currentRow, currentCol].Color == checker.Color)
                        return false;

                    if (!foundCapture)
                    {
                        foundCapture = true;
                        capturedRow = currentRow;
                        capturedCol = currentCol;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            PerformMove(checker, fromRow, fromCol, toRow, toCol);

            if (foundCapture)
            {
                Board[capturedRow, capturedCol] = null;

                if (CanKingJumpAgain(toRow, toCol))
                {
                    mustContinueJump = true;
                    return true;
                }
            }

            mustContinueJump = false;
            SwitchTurn();
            CheckForWin();
            return true;
        }




        return false;
    }

    private bool CanJumpAgain(int row, int col)
    {
        Checker checker = Board[row, col];
        if (checker == null)
        {
            return false;
        }

        int direction = (checker.Color == CheckerColor.White) ? 1 : -1; int[] moves = { -1, 1 };
        foreach (int move in moves)
        {
            int newRow = row + direction;
            int newCol = col + move;
            int jumpRow = row + (2 * direction);
            int jumpCol = col + (2 * move);

            if (IsValidPosition(jumpRow, jumpCol) &&
                Board[jumpRow, jumpCol] == null && Board[newRow, newCol] != null && Board[newRow, newCol].Color != checker.Color)
            {
                checker.HasCaptured = true;
                return true;
            }
        }

        foreach (int move in moves)
        {
            int newRow = row - direction; int newCol = col + move;
            int jumpRow = row - (2 * direction); int jumpCol = col + (2 * move);

            if (IsValidPosition(jumpRow, jumpCol) &&
                Board[jumpRow, jumpCol] == null && Board[newRow, newCol] != null && Board[newRow, newCol].Color != checker.Color)
            {
                checker.HasCaptured = true;
                return true;
            }
        }

        return false;
    }


    private bool CanKingJumpAgain(int row, int col)
    {
        Checker checker = Board[row, col];
        if (checker == null || checker.Type != CheckerType.King)
            return false;

        int[] rowDirs = { -1, 1 };
        int[] colDirs = { -1, 1 };

        foreach (var rowDir in rowDirs)
        {
            foreach (var colDir in colDirs)
            {
                int midRow = row + rowDir;
                int midCol = col + colDir;

                while (IsValidPosition(midRow, midCol))
                {
                    if (Board[midRow, midCol] != null && Board[midRow, midCol].Color != checker.Color)
                    {
                        int jumpRow = midRow + rowDir;
                        int jumpCol = midCol + colDir;

                        while (IsValidPosition(jumpRow, jumpCol))
                        {
                            if (Board[jumpRow, jumpCol] == null)
                                return true; jumpRow += rowDir;
                            jumpCol += colDir;
                        }
                        break;
                    }
                    midRow += rowDir;
                    midCol += colDir;
                }
            }
        }
        return false;
    }




    private void CheckForWin()
    {
        bool hasWhite = false;
        bool hasBlack = false;

        foreach (Checker piece in Board)
        {
            if (piece != null)
            {
                if (piece.Color == CheckerColor.White)
                {
                    hasWhite = true;
                }
                else if (piece.Color == CheckerColor.Black)
                {
                    hasBlack = true;
                }
            }

            if (hasWhite && hasBlack)
            {
                return;
            }
        }

        string winner = !hasWhite ? "Чёрные победили!" : "Белые победили!";
        PlaySound("spit-yeeeeeaaaahhh.mp3");
        _ = MessageBox.Show(winner, "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Information);
        Restart();
    }

    private void SwitchTurn()
    {
        if (!mustContinueJump)
        {
            CurrentTurn = (CurrentTurn == CheckerColor.White) ? CheckerColor.Black : CheckerColor.White;
        }

        if (!HasAvailableMoves(CurrentTurn))
        {
            CurrentTurn = (CurrentTurn == CheckerColor.White) ? CheckerColor.Black : CheckerColor.White;
        }

        TurnChanged?.Invoke(CurrentTurn);
    }

    private static bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < 8 && col >= 0 && col < 8;
    }

    private bool HasAvailableMoves(CheckerColor color)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (Board[row, col] != null && Board[row, col].Color == color)
                {
                    if (CanMove(row, col))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CanMove(int row, int col)
    {
        Checker checker = Board[row, col];
        if (checker == null)
        {
            return false;
        }

        int direction = (checker.Color == CheckerColor.White) ? 1 : -1;
        int[] moves = [-1, 1];

        foreach (int move in moves)
        {
            int newRow = row + direction;
            int newCol = col + move;

            if (IsValidPosition(newRow, newCol) && Board[newRow, newCol] == null)
            {
                return true;
            }

            int jumpRow = row + (2 * direction);
            int jumpCol = col + (2 * move);
            if (IsValidPosition(jumpRow, jumpCol) &&
                Board[jumpRow, jumpCol] == null &&
                Board[newRow, newCol] != null &&
                Board[newRow, newCol].Color != checker.Color)
            {
                return true;
            }
        }

        if (checker.Type == CheckerType.King)
        {
            for (int dRow = -1; dRow <= 1; dRow += 2)
            {
                for (int dCol = -1; dCol <= 1; dCol += 2)
                {
                    int newRow = row + dRow;
                    int newCol = col + dCol;

                    while (IsValidPosition(newRow, newCol))
                    {
                        if (Board[newRow, newCol] == null)
                        {
                            return true;
                        }

                        if (Board[newRow, newCol].Color != checker.Color &&
                            IsValidPosition(newRow + dRow, newCol + dCol) &&
                            Board[newRow + dRow, newCol + dCol] == null)
                        {
                            return true;
                        }

                        break;
                    }
                }
            }
        }

        return false;
    }
}
