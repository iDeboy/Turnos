using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Common;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
internal sealed class UserConfiguration : IDbConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.ToTable(DbConstants.UserTable);

        builder.Property(u => u.Name)
            .IsRequired();

        builder.HasDiscriminator<UserKind>(user => user.Kind)
               .HasValue<Alumno>(UserKind.Alumno)
               .HasValue<Personal>(UserKind.Personal)
               .IsComplete();
    }
}
