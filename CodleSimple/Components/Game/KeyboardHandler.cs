using CodleLogic;
using CodleWeb.Components.Pages;
using CodleWeb.Components.Services;
using Microsoft.AspNetCore.Components.Web;

namespace CodleWeb.Components.Game;

public class KeyboardHandler(
    GameBoard board,
    Codle codle,
    Validate validate,
    UpdateLetters updateLetters,
    Func<Task> focusCallback,
    IGameSessionService gameSessionService,
    Func<Task> statehaschanged,
    Home home)
{
  public async Task HandleKeyPress(KeyboardEventArgs evt)
    {
        await focusCallback.Invoke();
        board.CurrentGuess = board.CurrentGuess.ToLower();
        Console.WriteLine($"Key: {evt.Key}, Code: {evt.Code}");

        switch (evt.Code)
        {
            case "Backspace":
                HandleBackspace();
                return;
            case "Enter":
                await HandleEnter();
                return;
            case "Tab":
                await focusCallback.Invoke();
                return;
        }

        if (evt.Key.Length == 1 && char.IsLetter(evt.Key[0]))
        {
            HandleLetterInput(evt.Key[0]);
        }
    }

    public void HandleLetterInput(char key)
    {
        if (board.CurrentGuess.Length >= 5 || board.CurrentColumn >= 5) return;

        char upperKey = char.ToUpper(key);
        board.CurrentGuess += upperKey;
        board.Grid[board.CurrentRow, board.CurrentColumn] = upperKey;
        board.GridStyles[board.CurrentRow, board.CurrentColumn] = "typed";
        board.CurrentColumn++;
    }

    public void HandleBackspace()
    {
        if (board.CurrentGuess.Length == 0 || board.CurrentColumn == 0) return;

        board.CurrentGuess = board.CurrentGuess[..^1];
        board.CurrentColumn--;
        board.GridStyles[board.CurrentRow, board.CurrentColumn] = "";
        board.Grid[board.CurrentRow, board.CurrentColumn] = ' ';
    }

    public async Task HandleEnter()
    {
        if (board.CurrentGuess.Length != 5 || board.CurrentRow > 6 || !board.CurrentGuess.All(char.IsLetter)) return;
        if (!validate.CheckIfGuessIsValidWord(board.CurrentGuess)) return;

        codle.MakeGuess(board.CurrentGuess);
        updateLetters.CheckCorrectLetters(board.CurrentGuess);

        if (string.Equals(board.CurrentGuess, codle.CodleWord, StringComparison.OrdinalIgnoreCase))
        {
            await gameSessionService.ResetRestartCountAsync();
            home.UnfinishedRestartCount = await gameSessionService.GetUnfinishedRestartCountAsync();
            home.DidPlayerWin = true;
            home.FinishedGameFair = true;
            home.IsRestartBlocked = home.UnfinishedRestartCount >= 3;
            await statehaschanged.Invoke();
        }

        board.CurrentGuess = string.Empty;
        board.CurrentRow++;
        board.CurrentColumn = 0;

        if (board.CurrentRow >= 6)
        {
            await gameSessionService.ResetRestartCountAsync();
            home.UnfinishedRestartCount = await gameSessionService.GetUnfinishedRestartCountAsync();
            home.FinishedGameFair = true;
            home.IsRestartBlocked = home.UnfinishedRestartCount >= 3;
            await statehaschanged.Invoke();
        }
    }
}
