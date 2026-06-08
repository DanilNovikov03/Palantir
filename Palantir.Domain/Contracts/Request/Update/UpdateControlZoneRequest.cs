namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateControlZoneRequest(
        string PrecisionControl,
        MultiPolygon Geom
    );
}
