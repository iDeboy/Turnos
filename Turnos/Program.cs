using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Components;
using Turnos.Data;
using Turnos.Data.Auth;
using Turnos.EmailSenders;
using Turnos.Extensions;
using Turnos.Hubs;
using Turnos.Options;
using Turnos.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

builder.Services.AddSingleton(typeof(IStoreService<,>), typeof(StoreService<,>));
builder.Services.AddScoped<IAlumnoService, AlumnoService>();

builder.Services.AddDbContext<TurnosDbContext>();

builder.Services.AddIdentityServices();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<TurnosDbContext>()
    .UseCryptographicAlgorithms(new());

builder.Services.AddEmail(builder.Configuration);

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
}
else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Turnos.Client._Imports).Assembly);

app.MapIdentityEndpoints();

app.MapHub<AlumnoHub>(Paths.AlumnoHub);

app.Run();
