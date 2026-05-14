namespace Palantir.Domain.Abstraction.Application
{
    public interface IWarService
    {
        Task<List<WarResponse>> GetAllAsync();
        Task<WarResponse?> GetByIdAsync(int id);
        Task AddAsync(WarRequest warRequest);
        Task UpdateAsync(int id, WarRequest warRequest);
        Task DeleteAsync(int id);
    }
}
