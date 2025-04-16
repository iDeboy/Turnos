using Microsoft.AspNetCore.Components;

namespace Turnos.Client.Components;
public partial class Card {

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? TitleClass { get; set; }

    [Parameter]
    public RenderFragment? TitleTemplate { get; set; }

    [Parameter]
    public string? ContentClass { get; set; }

    [Parameter]
    public RenderFragment? ContentTemplate { get; set; }

    [Parameter]
    public string? ActionClass { get; set; }

    [Parameter]
    public RenderFragment? ActionTemplate { get; set; }

}
