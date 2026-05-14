namespace Palantir.Domain.Abstraction.Application
{
    public interface IOperationSideService
    {
        Task<List<OperationSideResponse>> GetAllAsync();
        Task<OperationSideResponse?> GetByIdsAsync(int operationId, int warSideId);
        Task<List<OperationSideResponse>> GetByOperationIdAsync(int operationId);
        Task<List<OperationSideResponse>> GetByWarSideIdAsync(int warSideId);

        Task AddAsync(CreateOperationSideRequest operationSideRequest);
        Task UpdateAsync(int operationId, int warSideId, UpdateOperationSideRequest operationSideRequest);
        Task DeleteAsync(int operationId, int warSideId);
    }
}
