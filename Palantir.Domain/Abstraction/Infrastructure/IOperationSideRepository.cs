namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IOperationSideRepository
    {
        Task<List<OperationSide>> GetAllAsync();
        Task<List<OperationSide>> GetByOperationIdAsync(int operationId);
        Task<List<OperationSide>> GetByWarSideIdAsync(int warSideId);
        Task<OperationSide?> GetByIdsAsync(int operationId, int warSideId);
        Task AddAsync(OperationSide operationSide);
        Task UpdateAsync(OperationSide operationSide);
        Task DeleteAsync(int operationId, int warSideid);
    }
}
