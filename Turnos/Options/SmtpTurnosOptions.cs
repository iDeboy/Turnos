using System.ComponentModel.DataAnnotations;

namespace Turnos.Options; 
public sealed class SmtpTurnosOptions {

    public const string Turnos = "Email";

    [Required]
    public string Host { get; set; } = default!;

    public int Port { get; set; } = 25;

    public bool EnableSSL { get; set; } = false;

    [Required]
    public string Address { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

}
