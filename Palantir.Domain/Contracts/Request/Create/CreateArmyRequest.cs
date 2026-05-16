namespace Palantir.Domain.Contracts.Request.Create
{
    public record class CreateArmyRequest(
        int WarSideId,
        string Name,
        string? TypeArmy,
        DateOnly? StartDate,
        DateOnly? EndDate,
        string? Summary
    );
}
