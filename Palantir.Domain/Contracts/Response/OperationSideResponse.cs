namespace Palantir.Domain.Contracts.Response
{
    public record class OperationSideResponse(
        int OperationId,
        int WarSideId,
        int SideId,
        string Title,
        string? ColorHex,
        string? RoleSide,
        string? Note
    );
}
