using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CodleWeb.Components.Elements;

public partial class VisibleKeyboard
{
    [Parameter] public List<List<string>> Rows { get; set; } = [];
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyClick { get; set; }
    [Parameter] public Dictionary<string, string> KeyboardStyle { get; set; } = [];

    public string LetterColorChange(string letter) =>
        (KeyboardStyle.TryGetValue(letter, out var style)) ? style : string.Empty;
}
