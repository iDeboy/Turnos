using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Components;
using Turnos.Data;
using Turnos.EmailSenders;
using Turnos.Events;
using Turnos.Extensions;
using Turnos.Hubs;
using Turnos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices(config => {
    config.SnackbarConfiguration.HideTransitionDuration = 100;
    config.SnackbarConfiguration.PreventDuplicates = false;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
});

builder.Services.AddSingleton(typeof(IStoreService<,>), typeof(StoreService<,>));
builder.Services.AddScoped(typeof(IScopedStoreService<,>), typeof(ScopedStoreService<,>));
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IPersonalService, PersonalService>();
builder.Services.AddSingleton<EventManager>();
builder.Services.AddSingleton<IEventProvider>(sp => sp.GetRequiredService<EventManager>());
builder.Services.AddSingleton<IEventNotifier>(sp => sp.GetRequiredService<EventManager>());

builder.Services.AddDbContext<TurnosDbContext>();

builder.Services.AddIdentityServices();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<TurnosDbContext>()
    .UseCryptographicAlgorithms(new());

builder.Services.AddEmail(builder.Configuration);

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

/*app.MapHub<AlumnoHub>(Paths.AlumnoHub);
app.MapHub<PersonalHub>(Paths.PersonalHub);*/
app.MapHub<TurnosHub>(Paths.TurnosHub);

app.Run();
