namespace Palantir.Domain.Contracts.Request.Update
{
    public record class EventPositionRequest(
        double Latitude,
        double Longitude
    );
}
