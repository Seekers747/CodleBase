namespace CodleWeb.Components.Game;

public class KeyboardHandler(GameBoard board)
{
    private readonly GameBoard _board = board;

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
