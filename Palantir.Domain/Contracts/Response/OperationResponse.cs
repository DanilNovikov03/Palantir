namespace Palantir.Domain.Contracts.Response
{
    public record class OperationResponse(
        int Id, 
        int TheaterId, 
        string Title, 
        DateOnly StartDate, 
        DateOnly EndDate,
        string? Summary
    );
}
