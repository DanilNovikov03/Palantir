namespace Palantir.Domain.Abstraction.Application
{
    public interface ITheaterService
    {
        Task<List<TheaterResponse>> GetAllAsync();
        Task<TheaterResponse?> GetByIdAsync(int theaterId);
        Task AddAsync(TheaterRequest request);
        Task UpdateAsync(int theaterId, TheaterRequest request);
        Task DeleteAsync(int theaterId);
    }
}
