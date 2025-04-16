using Turnos.Common;

namespace Turnos.Data.Auth;
public sealed class Personal : User {
    public override UserKind Kind { get; protected set; } = UserKind.Personal;

    public ICollection<Fila> Filas { get; set; } = [];
}
