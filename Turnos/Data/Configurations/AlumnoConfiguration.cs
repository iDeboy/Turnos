using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
public sealed class AlumnoConfiguration : IDbConfiguration<Alumno> {
    public void Configure(EntityTypeBuilder<Alumno> builder) {

        builder.HasMany(a => a.Turnos)
                .WithOne(t => t.Alumno)
                .HasForeignKey(t => t.AlumnoId);

    }
}
