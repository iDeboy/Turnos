namespace Turnos.Data.Auth;
public interface ITrackedEntity {

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

}
