namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<List<Event>> GetByOperationAsync(int operationId);
        Task<List<Event>> GetByWarDateAsync(int warId, DateOnly date, int? operationId);
        Task AddAsync(Event mapEvent);
        Task UpdateAsync(Event mapEvent);
        Task DeleteAsync(int id);
    }
}
