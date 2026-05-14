namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface ITheaterRepository
    {
        Task<List<Theater>> GetAllAsync();
        Task<Theater?> GetByIdAsync(int id);
        Task AddAsync(Theater theater);
        Task UpdateAsync(Theater theater);
        Task DeleteAsync(int id);
    }
}
