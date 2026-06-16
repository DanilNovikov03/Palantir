namespace Palantir.Domain.Abstraction.Application
{
    public interface IArmyService
    {
        Task<ArmyResponse?> GetByIdAsync(int id);
        Task<List<ArmyMapResponse>> GetByWarDateWithPositionsAsync(int warId, DateOnly date);
        Task<ArmyResponse> AddAsync(CreateArmyRequest armyRequest);
        Task<ArmyMapResponse> AddWithPositionAsync(CreateArmyWithPositionRequest armyRequest);
        Task<ArmyMapResponse?> UpdatePositionAsync(int armyId, UpdateArmyMapPositionRequest positionRequest);
        Task<ArmyResponse> UpdateAsync(int id, UpdateArmyRequest armyRequest);
        Task DeleteAsync(int id);
    }
}
