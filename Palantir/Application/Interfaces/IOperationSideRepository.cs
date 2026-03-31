using Palantir.Domain.Models;

namespace Palantir.Application.Interfaces
{
    public interface IOperationSideRepository
    {
        Task<List<OperationSide>> GetAllAsync();
    }
}
