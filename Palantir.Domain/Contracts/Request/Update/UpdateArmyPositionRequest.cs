namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateArmyPositionRequest(
        Point Coordinate,
        string? Note
    );
}
