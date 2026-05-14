namespace Palantir.Domain.Contracts.Request
{
    public record class WarRequest(
        string Title,
        DateOnly StartDate,
        DateOnly? EndDate,
        string? Summary
    );
}
