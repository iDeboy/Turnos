using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Turnos.Data.Auth;

namespace Turnos.Data.Configurations; 
public abstract class TrackingEntityConfiguration<T> : IDbConfiguration<T> where T : class, ITrackedEntity {
    public virtual void Configure(EntityTypeBuilder<T> builder) {

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(t => t.UpdatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

    }
}
