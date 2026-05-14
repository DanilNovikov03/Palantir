namespace Palantir.Domain.Abstraction.Application
{
    public interface IArmyService
    {
        Task<ArmyResponse?> GetByIdAsync(int id);
        Task AddAsync(ArmyRequest armyRequest);
        Task UpdateAsync(int id, ArmyRequest armyRequest);
        Task DeleteAsync(int id);
    }
}
