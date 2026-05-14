namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IArmyRepository
    {
        Task<List<Army>> GetAllAsync();
        Task<Army?> GetByIdAsync(int id);
        Task AddAsync(Army army);
        Task UpdateAsync(Army army);
        Task DeleteAsync(int id);
    }
}
