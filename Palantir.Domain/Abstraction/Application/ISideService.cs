namespace Palantir.Domain.Abstraction.Application
{
    public interface ISideService
    {
        Task<List<SideResponse>> GetAllAsync();
        Task<SideResponse?> GetByIdAsync(int id);
        Task<SideResponse> AddAsync(SideRequest sideRequest);
        Task<SideResponse> UpdateAsync(int id, SideRequest sideRequest);
        Task DeleteAsync(int id);
    }
}
