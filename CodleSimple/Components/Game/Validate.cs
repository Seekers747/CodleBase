namespace CodleWeb.Components.Game
{
    public class Validate
    {
        readonly string[] lines = [.. File.ReadAllLines("combined_wordlist.txt").OrderBy(line => line)];
        public bool CheckIfGuessIsValidWord(string ValidGuess) => Array.BinarySearch(lines, ValidGuess) > 0;
    }
}
