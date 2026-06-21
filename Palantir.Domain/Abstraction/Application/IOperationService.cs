namespace Palantir.Domain.Abstraction.Application
{
    public interface IOperationService
    {
        Task<OperationResponse?> GetByIdAsync(int id);
        Task<List<OperationMapSideResponse>?> GetSidesAsync(int operationId);
        Task<OperationMapSideResponse> AddSideAsync(int operationId, AddOperationSideRequest request);
        Task DeleteSideAsync(int operationId, int warSideId);
        Task<List<OperationResponse>?> GetByTheaterIdAsync(int id);
        Task<OperationResponse> AddAsync(OperationRequest operation);
        Task<OperationResponse> UpdateAsync(int id, OperationRequest operation);
        Task DeleteAsync(int id);
    }
}
