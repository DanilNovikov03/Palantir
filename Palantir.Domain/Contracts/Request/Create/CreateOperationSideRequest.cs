namespace Palantir.Domain.Contracts.Request.Create
{
    public record class CreateOperationSideRequest(
        int OperationId,
        int WarSideId,
        string? RoleSide,
        string? Note
    );
}
