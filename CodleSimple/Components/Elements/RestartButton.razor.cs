using Microsoft.AspNetCore.Components;
using CodleWeb.Components.Game;

namespace CodleWeb.Components.Elements;

public partial class RestartButton
{
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public CodleLogic.Codle Codle { get; set; } = default!;
    [Parameter] public GameBoard Board { get; set; } = default!;
    [Parameter] public Dictionary<string, string> KeyboardStyle { get; set; } = default!;
    [Parameter] public bool DidPlayerWin { get; set; }
    [Parameter] public bool FinishedGameFair { get; set; }
    [Parameter] public int UnfinishedRestartCount { get; set; }
    [Parameter] public Func<Task> FocusAction { get; set; } = default!;
    [Parameter] public Func<Task> IncrementRestart { get; set; } = default!;
    [Parameter] public Func<Task<int>> GetUnfinishedRestart { get; set; } = default!;
    [Parameter] public Func<Task> StateHasChangedCallback { get; set; } = default!;
    [Parameter] public Func<bool, Task> SetIsRestartBlocked { get; set; } = default!;

    private async Task HandleClick()
    {
        await RestartGameHelper.RestartAsync(
            Codle,
            Board,
            KeyboardStyle,
            DidPlayerWin,
            FocusAction,
            FinishedGameFair,
            UnfinishedRestartCount,
            IncrementRestart,
            GetUnfinishedRestart,
            StateHasChangedCallback,
            SetIsRestartBlocked
        );
    }
}
