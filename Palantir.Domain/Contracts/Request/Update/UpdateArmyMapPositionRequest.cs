namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateArmyMapPositionRequest(
        DateOnly DatePosition,
        Point Coordinate,
        string? Note
    );
}
