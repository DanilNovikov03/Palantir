namespace Palantir.Domain.Contracts.Request
{
    public record class EventRequest(
        int WarId,
        int? OperationId,
        int? WarSideId,
        string Title,
        string? Text,
        string? Type,
        DateOnly Date,
        double Latitude,
        double Longitude
    );
}
