namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IArmyRepository
    {
        Task<List<Army>> GetAllAsync();
        Task<Army?> GetByIdAsync(int id);
        Task<List<Army>> GetByWarDateWithPositionsAsync(int warId, DateOnly date);
        Task AddAsync(Army army);
        Task AddWithPositionAsync(Army army, ArmyPosition position);
        Task<Army?> UpsertPositionAsync(int armyId, ArmyPosition position);
        Task UpdateAsync(Army army);
        Task DeleteAsync(int id);
    }
}
