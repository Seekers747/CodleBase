using Microsoft.AspNetCore.Components;

namespace CodleWeb.Components.Elements;

public partial class VisibleKeyboard
{
    [Parameter] public IEnumerable<string> TopRow { get; set; } = [];
    [Parameter] public IEnumerable<string> MiddleRow { get; set; } = [];
    [Parameter] public IEnumerable<string> BottomRow { get; set; } = [];

    [Parameter] public EventCallback<string> OnKeyClick { get; set; }
    [Parameter] public Func<string, string>? LetterColorChange { get; set; }
}
