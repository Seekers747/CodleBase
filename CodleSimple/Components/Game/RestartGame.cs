namespace CodleWeb.Components.Game;

public static class RestartGameHelper
{
  public static async Task RestartAsync(
      CodleLogic.Codle codle,
      GameBoard board,
      Dictionary<string, string> keyboardStyle,
      bool DidPlayerWin,
      Func<Task> focusAction,
      bool FinishedGameFair,
      int UnfinishedRestartCount,
      Func<Task> IncrementRestart,
      Func<Task<int>> GetUnfinishedRestart,
      Func<Task> stateHasChanged,
      Action<bool> setIsRestartBlocked,
      Action<bool> setFinishedGameFair,
      Action<int> setUnfinishedRestartCount,
      Action<bool> setDidPlayerWin
      )
  {
    if (!FinishedGameFair)
    {
      await IncrementRestart();
      UnfinishedRestartCount = await GetUnfinishedRestart();
      setUnfinishedRestartCount(UnfinishedRestartCount);
    }

    setFinishedGameFair(false);

    bool isBlocked = UnfinishedRestartCount >= 3;
    setIsRestartBlocked(isBlocked);

    await stateHasChanged.Invoke();

    if (isBlocked)
    {
      await focusAction();
      return;
    }

    setDidPlayerWin(false);

    codle.Reset(DidPlayerWin);
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

    await focusAction();
    await stateHasChanged.Invoke();
  }
}