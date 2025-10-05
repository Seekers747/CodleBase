using CodleLogic;

namespace CodleWeb.Components.Game;

public class UpdateLetters(GameBoard board, Dictionary<string, string> KeyboardStyle, Codle codle)
{
    private readonly GameBoard _board = board;
    private readonly Dictionary<string, string> _keyboardStyle = KeyboardStyle;
    private readonly Codle _codle = codle;

    public void CheckCorrectLetters(string guess)
    {
        string target = _codle.CodleWord;
        var targetCounts = new Dictionary<char, int>();
        var matchedCounts = new Dictionary<char, int>();

        foreach (char letter in target)
            targetCounts[letter] = targetCounts.GetValueOrDefault(letter) + 1;

        for (int i = 0; i < guess.Length; i++)
        {
            char letter = guess[i];
            if (letter == target[i])
            {
                _board.GridStyles[_board.CurrentRow, i] = "correct";
                UpdateKeyboardStyle(letter, "CorrectLetter");
                matchedCounts[letter] = matchedCounts.GetValueOrDefault(letter) + 1;
            }
        }

        for (int i = 0; i < guess.Length; i++)
        {
            if (_board.GridStyles[_board.CurrentRow, i] == "correct") continue;

            char letter = guess[i];
            bool isInTarget = target.Contains(letter);
            int matchedSoFar = matchedCounts.GetValueOrDefault(letter);
            int allowedMatches = targetCounts.GetValueOrDefault(letter);

            if (isInTarget && matchedSoFar < allowedMatches)
            {
                _board.GridStyles[_board.CurrentRow, i] = "present";
                UpdateKeyboardStyle(letter, "PresentLetter");
                matchedCounts[letter] = matchedSoFar + 1;
            }
            else
            {
                _board.GridStyles[_board.CurrentRow, i] = "absent";
                UpdateKeyboardStyle(letter, "AbsentLetter");
            }
        }
    }

    private void UpdateKeyboardStyle(char letter, string style)
    {
        string key = letter.ToString().ToUpper();

        if (!_keyboardStyle.TryGetValue(key, out string? value) ||
            (style == "PresentLetter" && value == "AbsentLetter") ||
            style == "CorrectLetter")
        {
            value = style;
            _keyboardStyle[key] = value;
        }
    }
}
