namespace Palantir.Domain.Abstraction.Application
{
    public interface IArmyService
    {
        Task<ArmyResponse?> GetByIdAsync(int id);
        Task<ArmyResponse> AddAsync(CreateArmyRequest armyRequest);
        Task<ArmyResponse> UpdateAsync(int id, UpdateArmyRequest armyRequest);
        Task DeleteAsync(int id);
    }
}
