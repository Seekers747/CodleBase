using Microsoft.AspNetCore.Components;

namespace CodleWeb.Components.Elements;

public partial class RestartButton
{
  [Parameter]
  public EventCallback OnClick { get; set; }

  [Parameter]
  public bool Disabled { get; set; } = false;
}
