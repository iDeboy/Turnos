using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Turnos.Client.Components.Icons;
public abstract class IconComponentBase : ComponentBase {

    [Parameter]
    public string? Class { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? Attributes { get; set; }

}
