using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Turnos.Client.Auth;
using Turnos.Client.Services;
using Turnos.Common;
using Turnos.Common.Abstractions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore(options => {
    options.AddPolicy(Policies.IsAlumno, Policies.AlumnoPolicy);
    options.AddPolicy(Policies.IsPersonal, Policies.PersonalPolicy);
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, ClientAuthStateProvider>();  

builder.Services.AddScoped<IAlumnoService, AlumnoService>();

await builder.Build().RunAsync();
