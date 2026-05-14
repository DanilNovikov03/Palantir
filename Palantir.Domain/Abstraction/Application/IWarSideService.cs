using Palantir.Domain.Contracts.Request.Create;
using Palantir.Domain.Contracts.Request.Update;

namespace Palantir.Domain.Abstraction.Application
{
    public interface IWarSideService
    {
        Task<List<WarSideResponse>> GetAllAsync();
        Task<WarSideResponse?> GetByIdAsync(int warSideId);
        Task<List<WarSideResponse>> GetByWarIdAsync(int warId);
        Task<List<WarSideResponse>> GetBySideIdAsync(int sideId);
        Task<WarSideResponse?> GetByIdsAsync(int warId, int sideId);
        Task AddAsync(CreateWarSideRequest createWarSideRequest);
        Task UpdateAsync(int warSideId, UpdateWarSideRequest updateWarSideRequest);
        Task DeleteAsync(int warSideId);
    }
}
