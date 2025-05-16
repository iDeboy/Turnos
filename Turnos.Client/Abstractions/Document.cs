using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Turnos.Client.Abstractions; 
public static class Document {

    public static event Action<KeyboardEventArgs>? KeyPress;
    public static event Action<KeyboardEventArgs>? KeyDown;

    [JSInvokable]
    public static Task OnKeyDown(KeyboardEventArgs args) {
        KeyDown?.Invoke(args);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public static Task OnKeyPress(KeyboardEventArgs args) {
        KeyPress?.Invoke(args);
        return Task.CompletedTask;
    }

}
