
namespace Palantir.Application.Services
{
    public class OperationService : IOperationService
    {
        private readonly IOperationRepository _reposirory;
        private readonly ITheaterRepository _theaterRepos;
        private readonly IWarSideRepository _warSideRepository;
        private readonly IOperationSideRepository _operationSideRepository;

        public OperationService(
            IOperationRepository repository,
            ITheaterRepository theaterRepos,
            IWarSideRepository warSideRepository,
            IOperationSideRepository operationSideRepository)
        {
            _reposirory = repository;
            _theaterRepos = theaterRepos;
            _warSideRepository = warSideRepository;
            _operationSideRepository = operationSideRepository;
        }

        public async Task<OperationResponse?> GetByIdAsync(int id)
        {
            var oper = await _reposirory.GetByIdAsync(id);

            if (oper == null)
                return null;

            return Response(oper);
        }

        public async Task<List<OperationMapSideResponse>?> GetSidesAsync(int operationId)
        {
            var operation = await _reposirory.GetByIdAsync(operationId);
            if (operation == null)
                return null;

            var sides = await _reposirory.GetSidesAsync(operationId);

            return sides
                .Select(warSide => new OperationMapSideResponse(
                    warSide.war_side_id,
                    warSide.war_side_id,
                    warSide.side_id,
                    warSide.side.title,
                    warSide.color_hex))
                .ToList();
        }

        public async Task<OperationMapSideResponse> AddSideAsync(int operationId, AddOperationSideRequest request)
        {
            var operation = await _reposirory.GetByIdAsync(operationId);
            if (operation == null)
                throw new KeyNotFoundException("Операция не найдена.");

            var warSide = await _warSideRepository.GetByIdAsync(request.WarSideId);
            if (warSide == null)
                throw new KeyNotFoundException("Сторона конфликта не найдена.");

            if (warSide.war_id != operation.theater.war_id)
                throw new InvalidOperationException("Эта сторона не относится к конфликту текущей операции.");

            if (await _operationSideRepository.GetByIdsAsync(operationId, request.WarSideId) != null)
                throw new InvalidOperationException("Эта сторона уже добавлена к операции.");

            await _operationSideRepository.AddAsync(new OperationSide
            {
                operation_id = operationId,
                war_side_id = warSide.war_side_id
            });

            return new OperationMapSideResponse(
                warSide.war_side_id,
                warSide.war_side_id,
                warSide.side_id,
                warSide.side.title,
                warSide.color_hex);
        }

        public async Task DeleteSideAsync(int operationId, int warSideId)
        {
            if (await _reposirory.GetByIdAsync(operationId) == null)
                throw new KeyNotFoundException("Операция не найдена.");

            if (await _operationSideRepository.GetByIdsAsync(operationId, warSideId) == null)
                throw new KeyNotFoundException("Сторона операции не найдена.");

            await _operationSideRepository.DeleteAsync(operationId, warSideId);
        }

        public async Task<List<OperationResponse>?> GetByTheaterIdAsync(int theaterId)
        {
            var theaterExists = await _theaterRepos.ExistsAsync(theaterId);
            if (!theaterExists)
                return null;

            var operations = await _reposirory
                .GetByTheaterIdAsync(theaterId);

            return operations.Select(operation =>
                Response(operation)
            ).ToList();
        }

        public async Task<OperationResponse> AddAsync(OperationRequest operRequest)
        {
            var theater = await _theaterRepos.GetByIdAsync(operRequest.TheaterId);
            if (theater == null)
                throw new KeyNotFoundException("Theater not found");

            var oper = new Operation
            {
                theater_id = operRequest.TheaterId,
                theater = theater,
                title = operRequest.Title,
                start_date = operRequest.StartDate,
                end_date = operRequest.EndDate,
                summary = operRequest.Summary
            };

            await _reposirory.AddAsync(oper);

            return Response(oper);
        }

        public async Task<OperationResponse> UpdateAsync(int id, OperationRequest operRequest)
        {
            var oper = await _reposirory.GetByIdAsync(id);
            if (oper == null)
                Exception();

            var theater = await _theaterRepos.GetByIdAsync(operRequest.TheaterId);
            if (theater == null)
                throw new KeyNotFoundException("Theater not found");

            oper.theater_id = operRequest.TheaterId;
            oper.theater = theater;
            oper.title = operRequest.Title;
            oper.start_date = operRequest.StartDate;
            oper.end_date = operRequest.EndDate;
            oper.summary = operRequest.Summary;

            await _reposirory.UpdateAsync(oper);

            return Response(oper);
        }

        public async Task DeleteAsync(int id)
        {
            var oper = await _reposirory.GetByIdAsync(id);
            if (oper == null)
                Exception();

            await _reposirory.DeleteAsync(id);
        }

        private OperationResponse Response(Operation operation) =>
            new OperationResponse(
                operation.operation_id,
                operation.theater_id,
                operation.theater.war_id,
                operation.title,
                operation.start_date,
                operation.end_date,
                operation.summary
            );

        private void Exception() =>
            throw new KeyNotFoundException("operation not found");
    }
}
