namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateOperationSideRequest(
        string? RoleSide,
        string? Note
    );
}
