namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IWarSideRepository
    {
        Task<List<WarSide>> GetAllAsync();
        Task<WarSide?> GetByIdAsync(int id);
        Task<WarSide?> GetByWarAndSideAsync(int warId, int sideId);
        Task<List<WarSide>> GetByWarIdAsync(int warId);
        Task<List<WarSide>> GetBySideIdAsync(int sideId);
        Task<bool> IsUsedByOperationsAsync(int warSideId);
        Task AddAsync(WarSide warSide);
        Task UpdateAsync(WarSide warSide);
        Task DeleteAsync(int id);
    }
}
