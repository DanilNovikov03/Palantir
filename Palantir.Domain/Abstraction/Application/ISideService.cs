namespace Palantir.Domain.Abstraction.Application
{
    public interface ISideService
    {
        Task<SideResponse?> GetByIdAsync(int id);
        Task AddAsync(SideRequest sideRequest);
        Task UpdateAsync(int id, SideRequest sideRequest);
        Task DeleteAsync(int id);
    }
}
