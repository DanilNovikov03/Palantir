namespace Palantir.Domain.Contracts.Request
{
    public record class OperationRequest(
        string Title,
        DateOnly StartDate,
        DateOnly EndDate,
        string? Summary
    );
}
