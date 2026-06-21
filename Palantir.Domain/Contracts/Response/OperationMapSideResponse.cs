namespace Palantir.Domain.Contracts.Response
{
    public record class OperationMapSideResponse(
        int OperationSideId,
        int WarSideId,
        int SideId,
        string Title,
        string? ColorHex
    );
}
