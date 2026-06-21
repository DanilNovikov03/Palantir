namespace Palantir.Domain.Abstraction.Application
{
    public interface IWarService
    {
        Task<List<WarResponse>> GetAllAsync();
        Task<WarResponse?> GetByIdAsync(int id);
        Task<List<MapSideResponse>?> GetSidesAsync(int warId);
        Task<MapSideResponse> AddSideAsync(int warId, AddWarSideRequest request);
        Task DeleteSideAsync(int warId, int warSideId);
        Task<WarResponse> AddAsync(WarRequest warRequest);
        Task<WarResponse> UpdateAsync(int id, WarRequest warRequest);
        Task DeleteAsync(int id);
    }
}
