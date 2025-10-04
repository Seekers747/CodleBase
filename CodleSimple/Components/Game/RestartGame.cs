using CodleWeb.Services;

namespace CodleWeb.Components.Game;

public class RestartGameHelper
{
    public static async Task RestartAsync(
        CodleLogic.Codle codle, 
        GameBoard board, 
        Dictionary<string, string> keyboardStyle, 
        bool didPlayerWin, 
        Func<Task> focusAction,
        bool FinishedGameFair,
        int UnfinishedRestartCount,
        Func<Task> IncrementRestart,
        Func<Task<int>> GetUnfinishedRestart,
        Func<Task> stateHasChanged,
        Func<bool, Task> setIsRestartBlocked
        )
    {
        if (!FinishedGameFair)
        {
            await IncrementRestart();
            UnfinishedRestartCount = await GetUnfinishedRestart();
        }
        FinishedGameFair = false;
        await stateHasChanged.Invoke();
        bool isBLocked = UnfinishedRestartCount >= 3;
        await setIsRestartBlocked(isBLocked);
        if (isBLocked)
        {
            await focusAction();
            return;
        }
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