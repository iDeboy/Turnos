using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
public sealed class TurnoConfiguration : TrackingEntityConfiguration<Turno> {
    public override void Configure(EntityTypeBuilder<Turno> builder) {

        base.Configure(builder);

        builder.HasKey(t => new { t.AlumnoId, t.FilaId, t.Lugar });

        builder.HasOne(t => t.Fila)
            .WithMany(f => f.Turnos)
            .HasForeignKey(t => t.FilaId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(t => t.Alumno)
            .WithMany(a => a.Turnos)
            .HasForeignKey(t => t.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(t => t.Lugar)
                .IsRequired();

        builder.Property(t => t.Estado)
            .IsRequired();

    }
}
