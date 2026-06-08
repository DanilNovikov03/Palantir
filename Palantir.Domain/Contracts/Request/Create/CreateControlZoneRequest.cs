namespace Palantir.Domain.Contracts.Request.Create
{
    public record class CreateControlZoneRequest(
        int WarId,
        int WarSideId,
        DateOnly DateControl,
        string PrecisionControl,
        MultiPolygon Geom
    );
}
