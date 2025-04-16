using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
public sealed class PersonalConfiguration : IDbConfiguration<Personal> {
    public void Configure(EntityTypeBuilder<Personal> builder) {

        builder.HasMany(p => p.Filas)
            .WithOne(f => f.Personal)
            .HasForeignKey(f => f.PersonalId);

    }
}
