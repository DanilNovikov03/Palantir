namespace Palantir.Domain.Contracts.Request.Update
{
    public record class UpdateWarSideRequest(
        DateOnly JoinedDate,
        DateOnly? OutDate,
        string? Note
    );
}
