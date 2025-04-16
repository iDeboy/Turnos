using Microsoft.AspNetCore.Identity;
using Turnos.Common;

namespace Turnos.Data.Auth;
public sealed class Role : IdentityRole<Guid> {

    public Role() {
        Id = Guidv7.Create();
    }

}
