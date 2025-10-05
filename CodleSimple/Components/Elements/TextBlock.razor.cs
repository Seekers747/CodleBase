using Microsoft.AspNetCore.Components;

namespace CodleWeb.Components.Elements;

public partial class TextBlock
{
  [Parameter] public string Message { get; set; } = "";
  [Parameter] public bool GameOver { get; set; } = false;
  [Parameter] public string? CodleWordExplain { get; set; }
}
