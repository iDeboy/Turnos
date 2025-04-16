using System.ComponentModel.DataAnnotations;

namespace Turnos.Auth; 
public sealed class ResetPasswordModel {

    [Required(ErrorMessage = "Debes ingresar un correo válido.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes ingresar una contraseña.")]
    // [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

}
