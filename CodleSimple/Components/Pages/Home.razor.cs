using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;

namespace CodleWeb.Components.Pages;

public partial class Home
{
    string CurrentGuess = "";
    private readonly char[,] grid = new char[7, 6];
    private readonly string[,] gridStyles = new string[7, 6];
    private int CurrentColumn;
    private int CurrentRow;
    public readonly CodleLogic.Codle codle = new();
    public List<char> CheckedLetters { get; private set; } = [];
    private ElementReference CodleResetFix;
    private readonly string[] TopRowVisibleKeyboard = ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"];
    private readonly string[] MiddleRowVisibleKeyboard = ["A", "S", "D", "F", "G", "H", "J", "K", "L"];
    private readonly string[] BottomRowVisibleKeyboard = ["Z", "X", "C", "V", "B", "N", "M"];
    private readonly Dictionary<string, string> VisibleKeyboardStyle = [];
    private CancellationTokenSource? computerCancelSource;
    public bool DidPlayerWin = false;
    private bool FinishedGameFair;

    protected override void OnInitialized()
    {
        codle.StartGame();
        InitializeGrid();
    }

    private bool _initializedRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initializedRender)
        {
            await GameSessionService.InitializeAsync();
            _initializedRender = true;

            StateHasChanged();
        }
    }

    private void InitializeGrid()
    {
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }
    }

    private async Task OnPhysicalKeyboardClick(KeyboardEventArgs evt)
    {
        if (!codle.GameOver) await HandleKeyPress(evt);
    }

    private async Task OnVisibleKeyboardClick(string letter)
    {
        var evt = new KeyboardEventArgs { Key = letter, Code = letter };
        Console.WriteLine(evt);
        await HandleKeyPress(evt);
    }

    private async Task HandleKeyPress(KeyboardEventArgs evt)
    {
        CurrentGuess = CurrentGuess.ToLower();
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
                await CodleResetFix.FocusAsync();
                return;
        }

        if (evt.Key.Length == 1 && char.IsLetter(evt.Key[0]))
        {
            HandleLetterInput(evt.Key[0]);
        }
    }

    private void HandleLetterInput(char key)
    {
        if (CurrentGuess.Length >= 5 || CurrentColumn >= 5) return;

        char upperKey = char.ToUpper(key);
        CurrentGuess += upperKey;
        grid[CurrentRow, CurrentColumn] = upperKey;
        gridStyles[CurrentRow, CurrentColumn] = "typed";
        CurrentColumn++;
    }

    private void HandleBackspace()
    {
        if (CurrentGuess.Length == 0 || CurrentColumn == 0) return;

        CurrentGuess = CurrentGuess[..^1];
        CurrentColumn--;
        gridStyles[CurrentRow, CurrentColumn] = "";
        grid[CurrentRow, CurrentColumn] = ' ';
    }

    private async Task HandleEnter()
    {
        if (CurrentGuess.Length != 5 || CurrentRow > 6 || !CurrentGuess.All(char.IsLetter)) return;
        if (!CheckIfGuessIsValidWord(CurrentGuess)) return;

        codle.MakeGuess(CurrentGuess);
        CheckCorrectLetters(CurrentGuess);

        if (string.Equals(CurrentGuess, codle.CodleWord, StringComparison.OrdinalIgnoreCase))
        {
            await GameSessionService.ResetRestartCountAsync();
            UnfinishedRestartCount = await GameSessionService.GetUnfinishedRestartCountAsync();
            FinishedGameFair = true;
            DidPlayerWin = true;
        }

        CurrentGuess = string.Empty;
        CurrentRow++;
        CurrentColumn = 0;

        if (CurrentRow >= 6)
        {
            await GameSessionService.ResetRestartCountAsync();
            UnfinishedRestartCount = await GameSessionService.GetUnfinishedRestartCountAsync();
            FinishedGameFair = true;
            StateHasChanged();
        }
    }

    private void CheckCorrectLetters(string guess)
    {
        string target = codle.CodleWord;
        var targetCounts = new Dictionary<char, int>();
        var matchedCounts = new Dictionary<char, int>();

        foreach (char letter in target)
            targetCounts[letter] = targetCounts.GetValueOrDefault(letter) + 1;

        for (int i = 0; i < guess.Length; i++)
        {
            char letter = guess[i];
            if (letter == target[i])
            {
                gridStyles[CurrentRow, i] = "correct";
                UpdateKeyboardStyle(letter, "CorrectLetter");
                matchedCounts[letter] = matchedCounts.GetValueOrDefault(letter) + 1;
            }
        }

        for (int i = 0; i < guess.Length; i++)
        {
            if (gridStyles[CurrentRow, i] == "correct") continue;

            char letter = guess[i];
            bool isInTarget = target.Contains(letter);
            int matchedSoFar = matchedCounts.GetValueOrDefault(letter);
            int allowedMatches = targetCounts.GetValueOrDefault(letter);

            if (isInTarget && matchedSoFar < allowedMatches)
            {
                gridStyles[CurrentRow, i] = "present";
                UpdateKeyboardStyle(letter, "PresentLetter");
                matchedCounts[letter] = matchedSoFar + 1;
            }
            else
            {
                gridStyles[CurrentRow, i] = "absent";
                UpdateKeyboardStyle(letter, "AbsentLetter");
            }
        }
    }

    private void UpdateKeyboardStyle(char letter, string style)
    {
        string key = letter.ToString().ToUpper();

        if (!VisibleKeyboardStyle.TryGetValue(key, out string? value) ||
            (style == "PresentLetter" && value == "AbsentLetter") ||
            style == "CorrectLetter")
        {
            value = style;
            VisibleKeyboardStyle[key] = value;
        }
    }

    private async Task RestartGame()
    {
        await CheckGameFair();
        if (IsRestartBlocked)
        {
            await CodleResetFix.FocusAsync();
            return;
        }

        computerCancelSource?.Cancel();
        computerCancelSource = null;

        codle.Reset(DidPlayerWin);
        CurrentGuess = string.Empty;
        CurrentRow = 0;
        CurrentColumn = 0;
        VisibleKeyboardStyle.Clear();

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                grid[y, x] = ' ';
                gridStyles[y, x] = string.Empty;
            }
        }

        await CodleResetFix.FocusAsync();
    }

    private string LetterColorChange(string letter) =>
        (VisibleKeyboardStyle.TryGetValue(letter, out var style)) ? style : string.Empty;

    readonly static string[] lines = [.. File.ReadAllLines("combined_wordlist.txt").OrderBy(line => line)];

    private static bool CheckIfGuessIsValidWord(string ValidGuess) => Array.BinarySearch(lines, ValidGuess) > 0;

    public int UnfinishedRestartCount { get; set; }
    private bool IsRestartBlocked => UnfinishedRestartCount >= 3;

    public async Task CheckGameFair()
    {
        if (!FinishedGameFair)
        {
            await GameSessionService.IncrementRestartCountAsync();
            UnfinishedRestartCount = await GameSessionService.GetUnfinishedRestartCountAsync();

            Console.WriteLine($"Unfinished restart count: {UnfinishedRestartCount}");

            // Example: take action if suspicious
            if (UnfinishedRestartCount >= 3)
            {
                Console.WriteLine("Warning: player may be trying to cheese!");
                // TODO: you could log to server or show a warning
            }
        }
        FinishedGameFair = false;
        Console.WriteLine($"the reset count: {UnfinishedRestartCount}");
        Console.WriteLine($"restart should be blocked {IsRestartBlocked}");
        StateHasChanged();
    }
}