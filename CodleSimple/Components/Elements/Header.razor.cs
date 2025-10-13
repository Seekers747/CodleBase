using Microsoft.AspNetCore.Components;

namespace CodleWeb.Components.Elements;

public partial class Header
{
  [Parameter] public string Message { get; set; } = "";
  [Parameter] public bool GameOver { get; set; } = false;
  [Parameter] public string? CodleWordExplain { get; set; }
}
