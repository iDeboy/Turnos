using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Turnos.Data.Configurations;
internal sealed class RoleClaimConfiguration : IDbConfiguration<IdentityRoleClaim<Guid>> {
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder) {
        builder.ToTable(DbConstants.RoleClaimTable);
    }
}
