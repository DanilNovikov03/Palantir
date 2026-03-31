using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface IArmyRepository
    {
        Task<List<Army>> GetAllAsync();
        Task<Army?> GetByIdAsync(int id);
    }
}
