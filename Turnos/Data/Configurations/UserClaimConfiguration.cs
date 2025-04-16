using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Turnos.Data.Configurations;
internal sealed class UserClaimConfiguration : IDbConfiguration<IdentityUserClaim<Guid>> {
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder) {
        builder.ToTable(DbConstants.UserClaimTable);
    }
}
