namespace Palantir.Domain.Contracts.Response
{
    public record class WarResponse(
        int Id,
        string Title,
        DateOnly StartDate,
        DateOnly? EndDate,
        string? Summary
    );
}
