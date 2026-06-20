namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IOperationRepository
    {
        Task<List<Operation>> GetAllAsync();
        Task<List<Operation>> GetByTheaterIdAsync(int theaterId);
        Task<Operation?> GetByIdAsync(int id);
        Task<List<WarSide>> GetSidesAsync(int operationId);
        Task AddAsync(Operation operation);
        Task UpdateAsync(Operation operation);
        Task DeleteAsync(int id);
    }
}
