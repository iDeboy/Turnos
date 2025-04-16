using System.ComponentModel.DataAnnotations;

namespace Turnos.Auth;
public sealed class ForgotPasswordModel {

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

}
