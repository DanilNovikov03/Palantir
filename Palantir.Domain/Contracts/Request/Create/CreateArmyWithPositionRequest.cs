namespace Palantir.Domain.Contracts.Request.Create
{
    public record class CreateArmyWithPositionRequest(
        int WarSideId,
        string Name,
        string? TypeArmy,
        string? Summary,
        DateOnly DatePosition,
        Point Coordinate,
        string? PositionNote
    );
}
