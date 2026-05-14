namespace Palantir.Domain.Abstraction.Application
{
    public interface IOperationService
    {
        Task<OperationResponse?> GetByIdAsync(int id);
        Task AddAsync(OperationRequest operation);
        Task UpdateAsync(int id, OperationRequest operation);
        Task DeleteAsync(int id);
    }
}
