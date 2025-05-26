using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Turnos.Client.Auth;
using Turnos.Client.Services;
using Turnos.Common;
using Turnos.Common.Abstractions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices(config => {
    config.SnackbarConfiguration.HideTransitionDuration = 100;
    config.SnackbarConfiguration.ShowTransitionDuration = 100;
    config.SnackbarConfiguration.PreventDuplicates = false;
});

builder.Services.AddAuthorizationCore(options => {
    options.AddPolicy(Policies.IsAlumno, Policies.AlumnoPolicy);
    options.AddPolicy(Policies.IsPersonal, Policies.PersonalPolicy);
    options.AddPolicy(Policies.IsClient, Policies.ClientPolicy);
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, ClientAuthStateProvider>();

builder.Services.AddSingleton(typeof(IStoreService<,>), typeof(StoreService<,>));
builder.Services.AddScoped(typeof(IScopedStoreService<,>), typeof(ScopedStoreService<,>));
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IPersonalService, PersonalService>();

await builder.Build().RunAsync();
