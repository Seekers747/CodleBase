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
    internal UpdateLetters _updateLetters;
    internal Validate _validate;

    public Home()
    {
        _keyboardHandler = new KeyboardHandler(_board);
        _updateLetters = new UpdateLetters(_board, VisibleKeyboardStyle, codle);
        _validate = new Validate();
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
        if (!_validate.CheckIfGuessIsValidWord(_board.CurrentGuess)) return;

        codle.MakeGuess(_board.CurrentGuess);
        _updateLetters.CheckCorrectLetters(_board.CurrentGuess);

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
}