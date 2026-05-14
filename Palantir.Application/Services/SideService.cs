namespace Palantir.Application.Services
{
    public class SideService : ISideService
    {
        ISideRepository _repository;

        public SideService(ISideRepository repository)
        {
            _repository = repository;
        }

        public async Task<SideResponse?> GetByIdAsync(int id)
        {
            var side = await _repository.GetByIdAsync(id);

            if (side == null)
                return null;

            return new SideResponse(
                side.side_id,
                side.title
            );
        }

        public async Task AddAsync(SideRequest sideRequest)
        {
            var side = new Side
            {
                title = sideRequest.Title
            };

            await _repository.AddAsync(side);
        }

        public async Task UpdateAsync(int id, SideRequest sideRequest)
        {
            var side = await _repository.GetByIdAsync(id);
            if (side == null)
                Exception();

            side.title = sideRequest.Title;

            await _repository.UpdateAsync(side);
        }

        public async Task DeleteAsync(int id)
        {
            var side = await _repository.GetByIdAsync(id);
            if (side == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private void Exception() =>
            throw new KeyNotFoundException("side not found");
    }
}
