namespace Palantir.Application.Services
{
    public class SideService : ISideService
    {
        private readonly ISideRepository _repository;

        public SideService(ISideRepository repository) =>
            _repository = repository;

        public async Task<List<SideResponse>> GetAllAsync()
        {
            var sides = await _repository.GetAllAsync();

            return sides
                .OrderBy(side => side.title)
                .Select(Response)
                .ToList();
        }

        public async Task<SideResponse?> GetByIdAsync(int id)
        {
            var side = await _repository.GetByIdAsync(id);

            if (side == null)
                return null;

            return Response(side);
        }

        public async Task<SideResponse> AddAsync(SideRequest sideRequest)
        {
            var side = new Side
            {
                title = sideRequest.Title
            };

            await _repository.AddAsync(side);

            return Response(side);
        }

        public async Task<SideResponse> UpdateAsync(int id, SideRequest sideRequest)
        {
            var side = await _repository.GetByIdAsync(id);
            if (side == null)
                Exception();

            side.title = sideRequest.Title;

            await _repository.UpdateAsync(side);

            return Response(side);
        }

        public async Task DeleteAsync(int id)
        {
            var side = await _repository.GetByIdAsync(id);
            if (side == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private SideResponse Response(Side side) =>
            new SideResponse(
                side.side_id,
                side.title
            );

        private void Exception() =>
            throw new KeyNotFoundException("side not found");
    }
}
