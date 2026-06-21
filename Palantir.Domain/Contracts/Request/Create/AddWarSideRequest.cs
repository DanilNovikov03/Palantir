namespace Palantir.Domain.Contracts.Request.Create
{
    public record class AddWarSideRequest(
        int SideId,
        string? ColorHex
    );
}
