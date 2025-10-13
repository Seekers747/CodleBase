using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CodleWeb.Components.Game;

namespace CodleWeb.Components.Pages;

public partial class Home
{
    public readonly CodleLogic.Codle codle = new();
    private ElementReference CodleResetFix;
    private readonly List<List<string>> VisibleKeyboardRows =
    [
        ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"],
        ["A", "S", "D", "F", "G", "H", "J", "K", "L", "\u232B"],
        ["Z", "X", "C", "V", "B", "N", "M", "\u23CE"]
    ];
    private readonly Dictionary<string, string> VisibleKeyboardStyle = [];
    public bool DidPlayerWin { get; set; } = false;
    public bool FinishedGameFair { get; set; }
    public int UnfinishedRestartCount { get; set; }
    public bool IsRestartBlocked { get; set; } = false;


    internal GameBoard _board = new();
    internal KeyboardHandler? _keyboardHandler;
    internal UpdateLetters _updateLetters;
    internal Validate _validate;

    public Home()
    {
        _updateLetters = new UpdateLetters(_board, VisibleKeyboardStyle, codle);
        _validate = new Validate();
    }

    protected override void OnInitialized()
    {
        codle.StartGame();
        _board.Initialize();
        _keyboardHandler = new KeyboardHandler(
            _board, 
            codle, 
            _validate, 
            _updateLetters, 
            async () => await CodleResetFix.FocusAsync(), 
            GameSessionService, 
            () => InvokeAsync(StateHasChanged),
            this
        );
    }

    private bool _initializedRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initializedRender)
        {
            await GameSessionService.InitializeAsync();
            _initializedRender = true;

            await InvokeAsync(StateHasChanged);
        }
    }
}