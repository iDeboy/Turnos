using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Turnos.Data.Configurations;
internal sealed class UserRoleConfiguration : IDbConfiguration<IdentityUserRole<Guid>> {
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder) {
        builder.ToTable(DbConstants.UserRoleTable);
    }
}
