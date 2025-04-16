using Microsoft.AspNetCore.Identity;
using Turnos.Common;

namespace Turnos.Data.Auth;
public abstract class User : IdentityUser<Guid> {

    public required string Name { get; set; }

    public abstract UserKind Kind { get; protected set; }

    public User() {
        Id = Guidv7.Create();
    }

    public static User Create(string name, UserKind kind) {

        return kind switch {
            UserKind.Alumno => new Alumno() { Name = name },
            UserKind.Personal => new Personal() { Name = name },
            _ => throw new InvalidOperationException()
        };
    }
}
