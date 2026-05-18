namespace Palantir.Domain.Abstraction.Application
{
    public interface ITheaterService
    {
        Task<List<TheaterResponse>> GetAllAsync();
        Task<List<TheaterResponse>?> GetByWarIdAsync(int warId);
        Task<TheaterResponse?> GetByIdAsync(int theaterId);
        Task<TheaterResponse> AddAsync(TheaterRequest request);
        Task<TheaterResponse> UpdateAsync(int theaterId, TheaterRequest request);
        Task DeleteAsync(int theaterId);
    }
}
