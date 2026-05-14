using Palantir.Domain.Contracts.Request.Create;
using Palantir.Domain.Contracts.Request.Update;

namespace Palantir.Domain.Abstraction.Application
{
    public interface IArmyPositionService
    {
        Task AddAsync(CreateArmyPositionRequest armyPosRequest);
        Task UpdateAsync(int id, UpdateArmyPositionRequest armyPosRequest);
        Task DeleteAsync(int id);
    }
}
