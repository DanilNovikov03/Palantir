namespace Palantir.Domain.Contracts.Response
{
    public record class OperationSideResponse(
        int OperationId,
        int WarSideId,
        string? RoleSide,
        string? Note
    );
}
