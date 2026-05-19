
namespace Palantir.Application.Services
{
    public class OperationService : IOperationService
    {
        private readonly IOperationRepository _reposirory;
        private readonly ITheaterRepository _theaterRepos;

        public OperationService(
            IOperationRepository repository, ITheaterRepository theaterRepos)
        {
            _reposirory = repository;
            _theaterRepos = theaterRepos;
        }

        public async Task<OperationResponse?> GetByIdAsync(int id)
        {
            var oper = await _reposirory.GetByIdAsync(id);

            if (oper == null)
                return null;

            return Response(oper);
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
            var oper = new Operation
            {
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
                operation.title,
                operation.start_date,
                operation.end_date,
                operation.summary
            );

        private void Exception() =>
            throw new KeyNotFoundException("operation not found");
    }
}
