
using Palantir.Domain.Models;

namespace Palantir.Application.Services
{
    public class WarSideService : IWarSideService
    {
        IWarSideRepository _repository;

        public WarSideService(IWarSideRepository repository) =>
            _repository = repository;


        public async Task<List<WarSideResponse>> GetAllAsync()
        {
            var warSides = await _repository.GetAllAsync();

            return warSides.Select(warSide =>
                Response(warSide)
            ).ToList();
        }

        public async Task<WarSideResponse?> GetByIdAsync(int warSideId)
        {
            var warSide = await _repository.GetByIdAsync(warSideId);
            if (warSide == null)
                return null;

            return Response(warSide);
        }

        public async Task<List<WarSideResponse>> GetBySideIdAsync(int sideId)
        {
            var sides = await _repository
                .GetBySideIdAsync(sideId);

            return sides.Select(side =>
                Response(side)
            ).ToList();
        }

        public async Task<List<WarSideResponse>> GetByWarIdAsync(int warId)
        {
            var wars = await _repository
                .GetByWarIdAsync(warId);

            return wars.Select(war => 
                Response(war)
            ).ToList();
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

        private WarSideResponse Response(WarSide warSide) =>
            new WarSideResponse(
                warSide.war_side_id,
                warSide.war_id,
                warSide.side_id,
                warSide.joined_date,
                warSide.out_date,
                warSide.note
            );

        private void Exception() =>
            throw new KeyNotFoundException("war side not found");
    }
}
