namespace Palantir.Domain.Contracts.Response
{
    public record class MapSideResponse(
        int WarSideId,
        int SideId,
        string Title,
        string? ColorHex
    );
}
