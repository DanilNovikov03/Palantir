using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface ITheaterRepository
    {
        Task<List<Theater>> GetAllAsync();
        Task<Theater?> GetByIdAsync(int id);
    }
}
