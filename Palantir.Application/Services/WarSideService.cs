
namespace Palantir.Application.Services
{
    public class WarSideService : IWarSideService
    {
        IWarSideRepository _repository;

        public WarSideService(IWarSideRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WarSideResponse>> GetAllAsync()
        {
            var warSides = await _repository.GetAllAsync();

            return warSides.Select(warSide => new WarSideResponse(
                warSide.war_side_id,
                warSide.war_id,
                warSide.side_id,
                warSide.joined_date,
                warSide.out_date,
                warSide.note
            )).ToList();
        }

        public async Task<WarSideResponse?> GetByIdAsync(int warSideId)
        {
            var war = await _repository.GetByIdAsync(warSideId);
            if (war == null)
                return null;

            return new WarSideResponse(
                war.war_side_id,
                war.war_id,
                war.side_id,
                war.joined_date,
                war.out_date,
                war.note
            );
        }

        public async Task<List<WarSideResponse>> GetBySideIdAsync(int sideId)
        {
            var sides = await _repository
                .GetBySideIdAsync(sideId);

            return sides.Select(side => new WarSideResponse(
                side.war_side_id,
                side.war_id,
                side.side_id,
                side.joined_date,
                side.out_date,
                side.note
            )).ToList();
        }

        public async Task<List<WarSideResponse>> GetByWarIdAsync(int warId)
        {
            var wars = await _repository
                .GetByWarIdAsync(warId);

            return wars.Select(war => new WarSideResponse(
                war.war_side_id,
                war.war_id,
                war.side_id,
                war.joined_date,
                war.out_date,
                war.note
            )).ToList();
        }

        public Task<WarSideResponse?> GetByIdsAsync(int warId, int sideId)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(CreateWarSideRequest createWarSideRequest)
        {
            var warSide = new WarSide
            {
                war_id = createWarSideRequest.WarId,
                side_id = createWarSideRequest.SideId,
                joined_date = createWarSideRequest.JoinedDate,
                out_date = createWarSideRequest.OutDate,
                note = createWarSideRequest.Note,
            };

            await _repository.AddAsync(warSide);
        }

        public async Task UpdateAsync(int warSideId, UpdateWarSideRequest updateWarSideRequest)
        {
            var warSide = 
                await _repository.GetByIdAsync(warSideId);
            if (warSide == null)
                Exception();

            var ws = updateWarSideRequest;
            warSide.joined_date = ws.JoinedDate;
            warSide.out_date = ws.OutDate;
            warSide.note = ws.Note;

            await _repository.UpdateAsync(warSide);
        }

        public async Task DeleteAsync(int warSideId)
        {
            var warSide = await _repository.GetByIdAsync(warSideId);
            if (warSide == null)
                Exception();

            await _repository.DeleteAsync(warSideId);
        }

        private void Exception() =>
            throw new KeyNotFoundException("war side not found");
    }
}
