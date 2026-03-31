using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface ISideRepository
    {
        Task<List<Side>> GetAllAsync();
        Task<Side?> GetByIdAsync(int id);
    }
}
