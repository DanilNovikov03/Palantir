namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IOperationRepository
    {
        Task<List<Operation>> GetAllAsync();
        Task<Operation?> GetByIdAsync(int id);
        Task AddAsync(Operation operation);
        Task UpdateAsync(Operation operation);
        Task DeleteAsync(int id);
    }
}
