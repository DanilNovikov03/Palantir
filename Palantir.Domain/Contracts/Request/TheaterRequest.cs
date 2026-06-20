namespace Palantir.Domain.Contracts.Request
{
    public record class TheaterRequest(
        int WarId,
        string Title,
        string? Summary
    );
}
