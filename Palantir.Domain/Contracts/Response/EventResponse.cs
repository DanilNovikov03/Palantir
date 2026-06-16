namespace Palantir.Domain.Contracts.Response
{
    public record class EventResponse(
        int Id,
        int WarId,
        int? OperationId,
        int? WarSideId,
        string Title,
        string? Text,
        string? Type,
        DateOnly Date,
        double Latitude,
        double Longitude,
        string? SideTitle,
        string? ColorHex
    );
}
