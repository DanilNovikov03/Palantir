namespace Palantir.Domain.Abstraction.Application
{
    public interface IOperationService
    {
        Task<OperationResponse?> GetByIdAsync(int id);
        Task<OperationResponse> AddAsync(OperationRequest operation);
        Task<OperationResponse> UpdateAsync(int id, OperationRequest operation);
        Task DeleteAsync(int id);
    }
}
