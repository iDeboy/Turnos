using Microsoft.AspNetCore.Components;

namespace Turnos.Components.Layout;
public partial class Header {

    [Parameter]
    public string? Title { get; init; }
}
