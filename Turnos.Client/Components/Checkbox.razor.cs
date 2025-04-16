using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using Turnos.Common;

namespace Turnos.Client.Components;
public sealed partial class Checkbox {

    private Guid Id { get; } = Guidv7.Create();

    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<bool>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

}
