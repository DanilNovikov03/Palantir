namespace Palantir.Domain.Contracts.Response
{
    public record class ControlZoneGeometryForDateResponse(
        ControlZoneResponse ControlZone,
        bool IsNewVersion
    );
}
