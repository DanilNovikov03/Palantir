namespace Palantir.Domain.Contracts.Response
{
    public record class ControlZoneResponse(
        int Id,
        int WarId,
        int WarSideId,
        DateOnly DateControl,
        string PrecisionControl,
        MultiPolygon Geom
    );
}
