using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Turnos.Data.Configurations;
internal sealed class UserLoginConfiguration : IDbConfiguration<IdentityUserLogin<Guid>> {
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder) {
        builder.ToTable(DbConstants.UserLoginTable);
    }
}
