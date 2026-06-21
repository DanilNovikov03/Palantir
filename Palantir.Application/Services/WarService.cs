namespace Palantir.Application.Services
{
    public class WarService : IWarService
    {
        private readonly IWarRepository _repository;
        private readonly ISideRepository _sideRepository;
        private readonly IWarSideRepository _warSideRepository;

        public WarService(
            IWarRepository repository,
            ISideRepository sideRepository,
            IWarSideRepository warSideRepository)
        {
            _repository = repository;
            _sideRepository = sideRepository;
            _warSideRepository = warSideRepository;
        }


        public async Task<List<WarResponse>> GetAllAsync()
        {
            var wars = await _repository.GetAllAsync();

            return wars.Select(war => 
                Response(war)
            ).ToList();
        }

        public async Task<WarResponse?> GetByIdAsync(int id)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                return null;

            return Response(war);
        }

        public async Task<List<MapSideResponse>?> GetSidesAsync(int warId)
        {
            if (!await _repository.ExistsAsync(warId))
                return null;

            var sides = await _repository.GetSidesAsync(warId);

            return sides
                .Select(warSide => new MapSideResponse(
                    warSide.war_side_id,
                    warSide.side_id,
                    warSide.side.title,
                    warSide.color_hex))
                .ToList();
        }

        public async Task<MapSideResponse> AddSideAsync(int warId, AddWarSideRequest request)
        {
            if (!await _repository.ExistsAsync(warId))
                throw new KeyNotFoundException("Конфликт не найден.");

            var side = await _sideRepository.GetByIdAsync(request.SideId);
            if (side == null)
                throw new KeyNotFoundException("Сторона не найдена.");

            if (await _warSideRepository.GetByWarAndSideAsync(warId, request.SideId) != null)
                throw new InvalidOperationException("Эта сторона уже добавлена к конфликту.");

            var colorHex = string.IsNullOrWhiteSpace(request.ColorHex)
                ? null
                : request.ColorHex.Trim();

            if (colorHex != null && !System.Text.RegularExpressions.Regex.IsMatch(colorHex, "^#[0-9a-fA-F]{6}$"))
                throw new InvalidOperationException("Цвет должен быть указан в формате #RRGGBB.");

            var warSide = new WarSide
            {
                war_id = warId,
                side_id = side.side_id,
                side = side,
                color_hex = colorHex
            };

            await _warSideRepository.AddAsync(warSide);

            return new MapSideResponse(
                warSide.war_side_id,
                side.side_id,
                side.title,
                warSide.color_hex);
        }

        public async Task DeleteSideAsync(int warId, int warSideId)
        {
            if (!await _repository.ExistsAsync(warId))
                throw new KeyNotFoundException("Конфликт не найден.");

            var warSide = await _warSideRepository.GetByIdAsync(warSideId);
            if (warSide == null || warSide.war_id != warId)
                throw new KeyNotFoundException("Сторона конфликта не найдена.");

            if (await _warSideRepository.IsUsedByOperationsAsync(warSideId))
                throw new InvalidOperationException("Нельзя удалить сторону конфликта, пока она используется в операциях.");

            await _warSideRepository.DeleteAsync(warSideId);
        }

        public async Task<WarResponse> AddAsync(WarRequest warRequest)
        {
            var war = new War
            {
                title = warRequest.Title,
                start_date = warRequest.StartDate,
                end_date = warRequest.EndDate,
                summary = warRequest.Summary
            };

            await _repository.AddAsync(war);

            return Response(war);
        }

        public async Task<WarResponse> UpdateAsync(int id, WarRequest warRequest)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                Exception();

            war.title = warRequest.Title;
            war.start_date = warRequest.StartDate;
            war.end_date = warRequest.EndDate;
            war.summary = warRequest.Summary;

            await _repository.UpdateAsync(war);

            return Response(war);
        }

        public async Task DeleteAsync(int id)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private WarResponse Response(War war) =>
            new WarResponse(
                war.war_id,
                war.title,
                war.start_date,
                war.end_date,
                war.summary
            );

        private void Exception() =>
            throw new KeyNotFoundException("war not found");
    }
}
