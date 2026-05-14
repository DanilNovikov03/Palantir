namespace Palantir.Domain.Contracts.Request
{
    public record class ArmyRequest(
        int WarSideId,
        string Name,
        string? TypeArmy,
        DateOnly? StartDate,
        DateOnly? EndDate,
        string? Summary
    );
}
