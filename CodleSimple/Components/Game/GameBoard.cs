namespace CodleWeb.Components.Game;

public class GameBoard
{
    public char[,] Grid { get; }
    public string[,] GridStyles { get; }
    public int CurrentRow { get; set; }
    public int CurrentColumn { get; set; }
    public string CurrentGuess { get; set; } = string.Empty;

    public GameBoard(int rows = 6, int cols = 5)
    {
        Grid = new char[rows, cols];
        GridStyles = new string[rows, cols];
        Initialize();
    }

    public void Initialize()
    {
        for (int y = 0; y < Grid.GetLength(0); y++)
        {
            for (int x = 0; x < Grid.GetLength(1); x++)
            {
                Grid[y, x] = ' ';
                GridStyles[y, x] = string.Empty;
            }
        }
        CurrentRow = 0;
        CurrentColumn = 0;
        CurrentGuess = string.Empty;
    }
}