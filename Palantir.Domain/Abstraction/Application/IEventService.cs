namespace Palantir.Domain.Abstraction.Application
{
    public interface IEventService
    {
        Task<EventResponse?> GetByIdAsync(int id);
        Task<List<EventResponse>> GetByOperationAsync(int operationId);
        Task<List<EventResponse>> GetByWarDateAsync(int warId, DateOnly date, int? operationId);
        Task<EventResponse> AddAsync(EventRequest eventRequest);
        Task<EventResponse?> UpdateAsync(int id, EventRequest eventRequest);
        Task<EventResponse?> UpdatePositionAsync(int id, EventPositionRequest positionRequest);
        Task DeleteAsync(int id);
    }
}
