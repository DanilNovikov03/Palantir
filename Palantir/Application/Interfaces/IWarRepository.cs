using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface IWarRepository
    {
        Task<List<War>> GetAllAsync();
        Task<War?> GetByIdAsync(int id);
    }
}
