using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Linq.Expressions;
using Turnos.Common;

namespace Turnos.Client.Components;
public partial class InputField {

    private Guid Id { get; } = Guidv7.Create();

    [Parameter]
    public InputType InputType { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public string Autocomplete { get; set; } = "off";

    [Parameter]
    public bool Validate { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<string>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

}
