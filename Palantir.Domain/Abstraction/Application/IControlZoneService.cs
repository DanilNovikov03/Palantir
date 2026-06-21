namespace Palantir.Domain.Abstraction.Application
{
    public interface IControlZoneService
    {
        Task<ControlZoneResponse?> GetByIdAsync(int controlZoneId);
        Task<List<ControlZoneResponse>> GetByWarDateAsync(int warId, DateOnly date);
        Task<List<ControlZoneResponse>> GetByWarSideDateAsync(
            int warId,
            int warSideId, 
            DateOnly date);
        Task<ControlZoneResponse> AddAsync(CreateControlZoneRequest controlZoneRequest);
        Task<ControlZoneResponse> UpdateAsync(int controlZoneId, UpdateControlZoneRequest controlZoneRequest);
        Task<ControlZoneGeometryForDateResponse> UpdateGeometryForDateAsync(
            int controlZoneId,
            UpdateControlZoneGeometryForDateRequest request);
        Task DeleteAsync(int controlZoneId);
    }
}
