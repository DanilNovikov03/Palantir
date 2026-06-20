namespace Palantir.Domain.Contracts.Response
{
    public record class OperationResponse(
        int Id,
        int TheaterId,
        int WarId,
        string Title, 
        DateOnly StartDate, 
        DateOnly EndDate,
        string? Summary
    );
}
