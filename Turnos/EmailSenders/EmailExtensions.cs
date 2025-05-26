using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Identity;
using Turnos.Data.Auth;
using Turnos.Options;

namespace Turnos.EmailSenders;
internal static class EmailExtensions {

    public static IServiceCollection AddEmail(this IServiceCollection services, ConfigurationManager configuration) {

        var options = new SmtpTurnosOptions();

        configuration.GetRequiredSection(SmtpTurnosOptions.Turnos)
            .Bind(options);

        if (options is null) return services;

        services.AddFluentEmail(options.Address, "Gestor de Turnos")
            .AddMailKitSender(new() {
                Server = options.Host,
                Port = options.Port,
                Password = options.Password,
                UseSsl = options.EnableSSL,
                User = options.Address,
                RequiresAuthentication = true,
            });

        services.AddTransient<TurnosEmailSender>();
        services.AddTransient<IEmail<User>, TurnosEmailSender>(s => s.GetRequiredService<TurnosEmailSender>());
        services.AddTransient<IEmailSender<User>, TurnosEmailSender>(s => s.GetRequiredService<TurnosEmailSender>());

        return services;
    }

    public static async Task<SendResponse> SendAsync(this IFluentEmail email, int retries, CancellationToken cancellationToken = default) {

        var result = await email.SendAsync(cancellationToken);

        int tries = 0;
        while (!result.Successful && tries++ < retries) {
            result = await email.SendAsync(cancellationToken);
        }

        return result;
    }

}
