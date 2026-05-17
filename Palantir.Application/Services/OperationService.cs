namespace Palantir.Application.Services
{
    public class OperationService : IOperationService
    {
        IOperationRepository _reposirory;

        public OperationService(IOperationRepository repository) =>
            _reposirory = repository;

        public async Task<OperationResponse?> GetByIdAsync(int id)
        {
            var oper = await _reposirory.GetByIdAsync(id);

            if (oper == null)
                return null;

            return Response(oper);
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
