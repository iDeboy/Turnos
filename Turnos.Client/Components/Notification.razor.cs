using Microsoft.AspNetCore.Components;
using Turnos.Client.Utils;

namespace Turnos.Client.Components;
public partial class Notification {

    [Parameter]
    public NotificationType NotificationType { get; set; }

    [Parameter]
    public string? Label { get; set; }

}
