namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IWarRepository
    {
        Task<List<War>> GetAllAsync();
        Task<War?> GetByIdAsync(int id);
        Task AddAsync(War war);
        Task UpdateAsync(War war);
        Task DeleteAsync(int id);
    }
}
