namespace Palantir.Domain.Contracts.Request.Create
{
    public record class CreateArmyPositionRequest(
        int ArmyId,
        DateOnly DatePosition,
        Point Coordinate,
        string? Note
    );
}
