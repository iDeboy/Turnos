using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using Turnos.Common;

namespace Turnos.Client.Components;
public partial class UserKindSelector {

    [CascadingParameter] 
    private EditContext? EditContext { get; set; }

    [Parameter]
    public UserKind Value { get; set; }

    [Parameter]
    public Expression<Func<UserKind>>? ValueExpression { get; set; }

    [Parameter]
    public EventCallback<UserKind> ValueChanged { get; init; }

}
