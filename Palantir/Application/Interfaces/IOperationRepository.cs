using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface IOperationRepository
    {
        Task<List<Operation>> GetAllAsync();
        Task<Operation?> GetByIdAsync(int id);
    }
}
