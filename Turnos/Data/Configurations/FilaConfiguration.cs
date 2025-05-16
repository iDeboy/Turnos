using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Common;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations;
public sealed class FilaConfiguration : TrackingEntityConfiguration<Fila> {
    public override void Configure(EntityTypeBuilder<Fila> builder) {

        base.Configure(builder);

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired();

        builder.Property(f => f.PasswordHash)
            .IsRequired(false);

        builder.Property(f => f.Estado)
            .IsRequired();

        builder.HasOne(f => f.Personal)
            .WithMany(p => p.Filas)
            .HasForeignKey(f => f.PersonalId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(f => f.Turnos)
            .WithOne(t => t.Fila)
            .HasForeignKey(t => t.FilaId);

        builder.Property(f => f.EstimatedAttentionTime)
            .IsRequired();

    }
}
