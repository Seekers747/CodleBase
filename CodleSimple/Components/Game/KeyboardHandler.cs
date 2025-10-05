using Microsoft.AspNetCore.Components.Web;

namespace CodleWeb.Components.Game;

public class KeyboardHandler(GameBoard board,Func<Task> focusCallback, Func<Task> HandleEnter)
{
    private readonly GameBoard _board = board;
    private readonly Func<Task> _focusCallback = focusCallback;
    private readonly Func<Task> _handleEnterCallback = HandleEnter;

    public async Task HandleKeyPress(KeyboardEventArgs evt)
    {
        await _focusCallback.Invoke();
        _board.CurrentGuess = _board.CurrentGuess.ToLower();
        Console.WriteLine($"Key: {evt.Key}, Code: {evt.Code}");

        switch (evt.Code)
        {
            case "Backspace":
                HandleBackspace();
                return;
            case "Enter":
                await _handleEnterCallback();
                return;
            case "Tab":
                await _focusCallback.Invoke();
                return;
        }

        if (evt.Key.Length == 1 && char.IsLetter(evt.Key[0]))
        {
            HandleLetterInput(evt.Key[0]);
        }
    }

    public void HandleLetterInput(char key)
    {
        if (_board.CurrentGuess.Length >= 5 || _board.CurrentColumn >= 5) return;

        char upperKey = char.ToUpper(key);
        _board.CurrentGuess += upperKey;
        _board.Grid[_board.CurrentRow, _board.CurrentColumn] = upperKey;
        _board.GridStyles[_board.CurrentRow, _board.CurrentColumn] = "typed";
        _board.CurrentColumn++;
    }

    public void HandleBackspace()
    {
        if (_board.CurrentGuess.Length == 0 || _board.CurrentColumn == 0) return;

        _board.CurrentGuess = _board.CurrentGuess[..^1];
        _board.CurrentColumn--;
        _board.GridStyles[_board.CurrentRow, _board.CurrentColumn] = "";
        _board.Grid[_board.CurrentRow, _board.CurrentColumn] = ' ';
    }
}
