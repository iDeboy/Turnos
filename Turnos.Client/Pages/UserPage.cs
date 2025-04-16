using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace Turnos.Client.Pages; 
public abstract class UserPage : ComponentBase {

    [Parameter]
    [EditorRequired]
    public required ClaimsPrincipal User { get; init; }   

}
