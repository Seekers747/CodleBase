using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Immutable;
using CodleWeb.Components.Game;

namespace CodleWeb.Components.Pages;

public partial class Home
{
    public readonly CodleLogic.Codle codle = new();
    public List<char> CheckedLetters { get; private set; } = [];
    private ElementReference CodleResetFix;
    private readonly string[] TopRowVisibleKeyboard = ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"];
    private readonly string[] MiddleRowVisibleKeyboard = ["A", "S", "D", "F", "G", "H", "J", "K", "L"];
    private readonly string[] BottomRowVisibleKeyboard = ["Z", "X", "C", "V", "B", "N", "M"];
    private readonly Dictionary<string, string> VisibleKeyboardStyle = [];
    public bool DidPlayerWin = false;
    private bool FinishedGameFair;
    public int UnfinishedRestartCount { get; set; }
    public bool IsRestartBlocked { get; set; } = false;


    internal GameBoard _board = new();
    internal KeyboardHandler _keyboardHandler;

    public Home()
    {
        _keyboardHandler = new KeyboardHandler(_board);
    }

    protected override void OnInitialized()
    {
        codle.StartGame();
        _board.Initialize();
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
        _board.CurrentGuess = _board.CurrentGuess.ToLower();
        Console.WriteLine($"Key: {evt.Key}, Code: {evt.Code}");

        switch (evt.Code)
        {
            case "Backspace":
                _keyboardHandler.HandleBackspace();
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
            _keyboardHandler.HandleLetterInput(evt.Key[0]);
        }
    }

    private async Task HandleEnter()
    {
        if (_board.CurrentGuess.Length != 5 || _board.CurrentRow > 6 || !_board.CurrentGuess.All(char.IsLetter)) return;
        if (!CheckIfGuessIsValidWord(_board.CurrentGuess)) return;

        codle.MakeGuess(_board.CurrentGuess);
        CheckCorrectLetters(_board.CurrentGuess);

        if (string.Equals(_board.CurrentGuess, codle.CodleWord, StringComparison.OrdinalIgnoreCase))
        {
            await GameSessionService.ResetRestartCountAsync();
            UnfinishedRestartCount = await GameSessionService.GetUnfinishedRestartCountAsync();
            FinishedGameFair = true;
            DidPlayerWin = true;
            IsRestartBlocked = UnfinishedRestartCount >= 3;
            StateHasChanged();
        }

        _board.CurrentGuess = string.Empty;
        _board.CurrentRow++;
        _board.CurrentColumn = 0;

        if (_board.CurrentRow >= 6)
        {
            await GameSessionService.ResetRestartCountAsync();
            UnfinishedRestartCount = await GameSessionService.GetUnfinishedRestartCountAsync();
            FinishedGameFair = true;
            IsRestartBlocked = UnfinishedRestartCount >= 3;
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
        await RestartGameHelper.RestartAsync(
            codle,
            _board,
            VisibleKeyboardStyle,
            DidPlayerWin,
            async () => await CodleResetFix.FocusAsync(),
            FinishedGameFair,
            UnfinishedRestartCount,
            async () => await GameSessionService.IncrementRestartCountAsync(),
            async () => await GameSessionService.GetUnfinishedRestartCountAsync(),
            () => { StateHasChanged(); return Task.CompletedTask; },
            (val) => { IsRestartBlocked = val; StateHasChanged(); return Task.CompletedTask; }
        );
    }

    private string LetterColorChange(string letter) =>
        (VisibleKeyboardStyle.TryGetValue(letter, out var style)) ? style : string.Empty;

    readonly static string[] lines = [.. File.ReadAllLines("combined_wordlist.txt").OrderBy(line => line)];

    private static bool CheckIfGuessIsValidWord(string ValidGuess) => Array.BinarySearch(lines, ValidGuess) > 0;
}