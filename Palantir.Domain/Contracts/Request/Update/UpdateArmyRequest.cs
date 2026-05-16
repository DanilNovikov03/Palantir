namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateArmyRequest(
        string Name,
        string? TypeArmy,
        DateOnly? StartDate,
        DateOnly? EndDate,
        string? Summary
    );
}
