using Turnos.Common;

namespace Turnos.Data.Auth;
public sealed class Alumno : User {
    public override UserKind Kind { get; protected set; } = UserKind.Alumno;

    public ICollection<Turno> Turnos { get; set; } = [];

}
