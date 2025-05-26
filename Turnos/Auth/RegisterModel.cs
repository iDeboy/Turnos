using System.ComponentModel.DataAnnotations;
using Turnos.Common;

namespace Turnos.Auth;
public sealed class RegisterModel {

    [Required]
    [EnumDataType(typeof(UserKind))]
    public UserKind Kind { get; set; } = UserKind.Alumno;

    [Required(ErrorMessage = "Debes ingresar tu nombre completo.")]
    [MinLength(2, ErrorMessage = "Nombre demaciado corto.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes ingresar un correo válido.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes ingresar una contraseña.")]
    // [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Debes confirmar la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string MasterKey { get; set; } = string.Empty;
}
