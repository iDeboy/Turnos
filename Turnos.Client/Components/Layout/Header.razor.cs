using Microsoft.AspNetCore.Components;

namespace Turnos.Client.Components.Layout;
public partial class Header {

    [Parameter]
    public string? Title { get; init; }
}
