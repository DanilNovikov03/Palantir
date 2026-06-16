namespace Palantir.Domain.Contracts.Response
{
    public record class ArmyMapResponse(
        int Id,
        int WarSideId,
        string? SideTitle,
        string Name,
        string? TypeArmy,
        string? Summary,
        int PositionId,
        DateOnly DatePosition,
        Point Coordinate,
        string? PositionNote
    );
}
