namespace Palantir.Domain.Abstraction.Application
{
    public interface ITheaterService
    {
        Task<List<TheaterResponse>> GetAllAsync();
        Task<TheaterResponse?> GetByIdAsync(int theaterId);
        Task<TheaterResponse> AddAsync(TheaterRequest request);
        Task<TheaterResponse> UpdateAsync(int theaterId, TheaterRequest request);
        Task DeleteAsync(int theaterId);
    }
}
