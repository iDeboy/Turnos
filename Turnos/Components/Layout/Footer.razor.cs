using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Turnos.Components.Layout;
public partial class Footer : IDisposable {

    private string? currentUrl;

    [Parameter]
    public string? Copyright { get; init; }

    
    protected override void OnInitialized() {
        currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
        currentUrl = NavigationManager.ToBaseRelativePath(e.Location);
        StateHasChanged();
    }

    public void Dispose() {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

}
