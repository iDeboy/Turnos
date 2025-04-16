using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Turnos.Data.Configurations;
internal sealed class UserTokenConfiguration : IDbConfiguration<IdentityUserToken<Guid>> {
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder) {
        builder.ToTable(DbConstants.UserTokenTable);
    }
}
