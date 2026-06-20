namespace Palantir.Domain.Contracts.Request
{
    public record class OperationRequest(
        int TheaterId,
        string Title,
        DateOnly StartDate,
        DateOnly EndDate,
        string? Summary
    );
}
