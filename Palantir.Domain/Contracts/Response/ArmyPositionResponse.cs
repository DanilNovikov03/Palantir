namespace Palantir.Domain.Contracts.Response
{
    public record class ArmyPositionResponse(int Id, int ArmyId, DateOnly DatePosition, Point Coordinate, string? Note);
}
