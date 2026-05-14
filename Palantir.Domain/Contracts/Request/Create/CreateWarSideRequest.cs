namespace Palantir.Domain.Contracts.Request.Create
{
    public record class CreateWarSideRequest(
        int WarId,
        int SideId,
        DateOnly JoinedDate,
        DateOnly? OutDate,
        string? Note
    );
}
