namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface ISideRepository
    {
        Task<List<Side>> GetAllAsync();
        Task<Side?> GetByIdAsync(int id);
        Task AddAsync(Side side);
        Task UpdateAsync(Side side);
        Task DeleteAsync(int id);
    }
}
