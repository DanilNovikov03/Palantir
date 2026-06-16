namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IControlZoneRepository
    {
        Task<ControlZone?> GetByIdAsync(int id);
        Task<List<ControlZone>> GetByWarDateAsync(int warId, DateOnly date);
        Task<List<ControlZone>> GetByWarSideDateAsync(
            int warId, 
            int warSideId, 
            DateOnly date);
        Task AddAsync(ControlZone controlZone);
        Task UpdateAsync(ControlZone controlZone);
        Task DeleteAsync(int id);
    }
}
