using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
internal sealed class RoleConfiguration : IDbConfiguration<Role> {
    public void Configure(EntityTypeBuilder<Role> builder) {
        builder.ToTable(DbConstants.RoleTable);
    }
}
