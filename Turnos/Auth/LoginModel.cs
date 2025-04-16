using System.ComponentModel.DataAnnotations;
using Turnos.Common;

namespace Turnos.Auth; 
public sealed class LoginModel {

    [Required(ErrorMessage = "Debes ingresar tu correo.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes ingresar tu contraseña.")]
    [DataType(DataType.Password, ErrorMessage = "Contraseña inválida.")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }

}
