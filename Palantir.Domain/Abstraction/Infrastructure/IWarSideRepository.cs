namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IWarSideRepository
    {
        Task<List<WarSide>> GetAllAsync();
        Task<WarSide?> GetByIdAsync(int id);
        Task<List<WarSide>> GetByWarIdAsync(int warId);
        Task<List<WarSide>> GetBySideIdAsync(int sideId);
        Task AddAsync(WarSide warSide);
        Task UpdateAsync(WarSide warSide);
        Task DeleteAsync(int id);
    }
}
