namespace Palantir.Domain.Abstraction.Infrastructure
{
    public interface IArmyPositionRepository
    {
        Task<List<ArmyPosition>> GetAllAsync();
        Task<ArmyPosition?> GetByIdAsync(int id);
        Task AddAsync(ArmyPosition position);
        Task UpdateAsync(ArmyPosition position);
        Task DeleteAsync(int id);
    }
}
