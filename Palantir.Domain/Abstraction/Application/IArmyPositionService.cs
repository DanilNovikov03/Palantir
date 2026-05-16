using Palantir.Domain.Contracts.Request.Create;
using Palantir.Domain.Contracts.Request.Update;

namespace Palantir.Domain.Abstraction.Application
{
    public interface IArmyPositionService
    {
        Task<ArmyPositionResponse> AddAsync(CreateArmyPositionRequest armyPosRequest);
        Task<ArmyPositionResponse> UpdateAsync(int id, UpdateArmyPositionRequest armyPosRequest);
        Task DeleteAsync(int id);
    }
}
