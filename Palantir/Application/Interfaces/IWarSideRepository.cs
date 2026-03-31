using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface IWarSideRepository
    {
        Task<List<WarSide>> GetAllAsync();
    }
}
