using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace Turnos.Client.Abstractions; 
public abstract class UserComponent : CancellableComponent {

    [Parameter]
    [EditorRequired]
    public required ClaimsPrincipal User { get; init; }   

}
