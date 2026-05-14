namespace Palantir.Domain.Contracts.Request
{
    public record class TheaterRequest(
        string Title,
        string? Summary
    );
}
