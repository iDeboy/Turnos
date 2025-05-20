using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Common;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
internal sealed class RoleConfiguration : IDbConfiguration<Role> {
    public void Configure(EntityTypeBuilder<Role> builder) {
        builder.ToTable(DbConstants.RoleTable);

        builder.HasData([
            new Role { Id = Guidv7.Create(), Name = Roles.Alumno, NormalizedName = Roles.Alumno.ToUpper() },
            new Role { Id = Guidv7.Create(), Name = Roles.Personal, NormalizedName = Roles.Personal.ToUpper() },
            ]);

    }
}
