namespace CodleWeb.Components.Game;

public class RestartGameHelper
{
    public static async Task RestartAsync(CodleLogic.Codle codle, GameBoard board, Dictionary<string, string> keyboardStyle, bool didPlayerWin, Func<Task> focusAction)
    {
        codle.Reset(didPlayerWin);
        board.CurrentGuess = string.Empty;
        board.CurrentRow = 0;
        board.CurrentColumn = 0;
        keyboardStyle.Clear();

        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 5; x++)
            {
                board.Grid[y, x] = ' ';
                board.GridStyles[y, x] = string.Empty;
            }

        if (focusAction != null)
            await focusAction.Invoke();
    }
}