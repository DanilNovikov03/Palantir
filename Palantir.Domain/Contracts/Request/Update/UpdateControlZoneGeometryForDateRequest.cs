namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateControlZoneGeometryForDateRequest(
        DateOnly Date,
        MultiPolygon? Geom
    );
}
