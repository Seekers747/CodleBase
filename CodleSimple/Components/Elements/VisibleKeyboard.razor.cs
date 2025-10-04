using CodleWeb.Components.Game;
using CodleWeb.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace CodleWeb.Components.Elements;

public partial class VisibleKeyboard
{
    [Parameter] public IEnumerable<string> TopRow { get; set; } = [];
    [Parameter] public IEnumerable<string> MiddleRow { get; set; } = [];
    [Parameter] public IEnumerable<string> BottomRow { get; set; } = [];

    [Parameter] public EventCallback<string> OnKeyClick { get; set; }
    [Parameter] public Dictionary<string, string> KeyboardStyle { get; set; } = [];

    public string LetterColorChange(string letter) =>
        (KeyboardStyle.TryGetValue(letter, out var style)) ? style : string.Empty;
}
