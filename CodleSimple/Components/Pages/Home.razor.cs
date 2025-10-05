using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CodleWeb.Components.Game;

namespace CodleWeb.Components.Pages;

public partial class Home
{
    public readonly CodleLogic.Codle codle = new();
    public List<char> CheckedLetters { get; private set; } = [];
    private ElementReference CodleResetFix;
    private readonly List<List<string>> VisibleKeyboardRows =
    [
        ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"],
        ["A", "S", "D", "F", "G", "H", "J", "K", "L", "Backspace"],
        ["Z", "X", "C", "V", "B", "N", "M", "Enter"]
    ];
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
        _keyboardHandler = new KeyboardHandler(_board, async () => await CodleResetFix.FocusAsync(), async () => await HandleEnter());
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

    public async Task HandleEnter()
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
}