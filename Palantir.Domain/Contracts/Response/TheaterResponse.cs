namespace Palantir.Domain.Contracts.Response
{
    public record class TheaterResponse(
        int Id,
        int WarId,
        string Title,
        string? Summary
    );
}
