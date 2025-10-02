using CodleLogic;

namespace CodleLogic;

public class Codle
{
    public string CodleWord { get; private set; } = string.Empty;
    public string CodleWordExplain { get; private set; } = string.Empty;
    private int ChancesLeft = 6;
    public string Message { get; private set; } = "Waiting for your guess...";
    public bool GameOver { get; private set; } = false;
    private string? UserChosenWord = null;

    public void StartGame()
    {
        ChancesLeft = 6;
        Message = "Waiting for your guess...";
        GameOver = false;
        LoadRandomCodleAnswer();
    }

    public void MakeGuess(string guess)
    {
        guess = guess.ToLower();

        if (GameOver) return;

        if (!IsValidGuess(guess))
        {
            Message = "Not a valid answer, the word must contain 5 letters!";
            return;
        }

        if (guess == CodleWord)
        {
            Message = "You guessed the word!";
            GameOver = true;
            return;
        }

        ChancesLeft--;

        GameOver = ChancesLeft == 0;
        Message = GameOver
            ? $"You didn't guess the word!<br /> The correct word was: {CodleWord}!"
            : $"Not the right word, you have {ChancesLeft} guesses left!";
    }

    private void LoadRandomCodleAnswer()
    {
        var lines = File.ReadAllLines("ExpandedWordList.txt");
        var random = new Random();
        int index = random.Next(lines.Length);
        var parts = lines[index].Split(':');

        if (!string.IsNullOrWhiteSpace(UserChosenWord))
        {
            CodleWord = UserChosenWord;

            string? match = lines.FirstOrDefault(line =>
                line.StartsWith(UserChosenWord + ":", StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                CodleWordExplain = "Geen uitleg beschikbaar.";
            }
            else
            {
                var userParts = match.Split(':');
                CodleWordExplain = userParts.Length > 1 ? userParts[1].Trim() : "Geen uitleg beschikbaar.";
            }

            UserChosenWord = null;
            return;
        }
        else
        {
            CodleWord = parts[0].Trim().ToLower();
            CodleWordExplain = parts.Length > 1 ? parts[1].Trim() : "Geen uitleg beschikbaar.";
        }
    }

    public void Reset(bool DidPlayerWin)
    {
        ChancesLeft = 6;
        Message = "Waiting for your guess...";
        GameOver = false;
        if (DidPlayerWin)
        {
            LoadRandomCodleAnswer();
        }
    }

    private static bool IsValidGuess(string guess)
    {
        return !string.IsNullOrWhiteSpace(guess) &&
               guess.Length == 5 &&
               !guess.All(char.IsDigit);
    }

    public static List<string> LoadAllWords()
    {
        return [.. File.ReadAllLines("ExpandedWordList.txt")
            .Select(line => line.Split(':')[0].Trim().ToLower())
            .Where(word => word.Length == 5)];
    }
}