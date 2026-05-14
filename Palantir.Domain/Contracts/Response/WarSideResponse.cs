namespace Palantir.Domain.Contracts.Response
{
    public record class WarSideResponse(
        int Id,
        int WarId,
        int SideId,
        DateOnly? JoinedDate,
        DateOnly? OutDate,
        string? Note
    );
}
