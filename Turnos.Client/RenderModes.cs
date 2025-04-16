using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Turnos.Client; 
public static class RenderModes {

    /// <summary>
    /// Gets an <see cref="IComponentRenderMode"/> that represents rendering interactively on the server via Blazor Server hosting.
    /// </summary>
    public static InteractiveServerRenderMode InteractiveServer { get; } = new(false);

    /// <summary>
    /// Gets an <see cref="IComponentRenderMode"/> that represents rendering interactively on the client via Blazor WebAssembly hosting.
    /// </summary>
    public static InteractiveWebAssemblyRenderMode InteractiveWebAssembly { get; } = new();

    /// <summary>
    /// Gets an <see cref="IComponentRenderMode"/> that means the render mode will be determined automatically based on a policy.
    /// </summary>
    public static InteractiveAutoRenderMode InteractiveAuto { get; } = new(false);
}
