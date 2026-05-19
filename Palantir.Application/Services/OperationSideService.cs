namespace Palantir.Application.Services
{
    public class OperationSideService : IOperationSideService
    {
        private readonly IOperationSideRepository _repository;

        public OperationSideService(IOperationSideRepository repository) =>
            _repository = repository;

        public async Task<List<OperationSideResponse>> GetAllAsync()
        {
            var operSides =
                await _repository.GetAllAsync();

            return operSides.Select(operSide => 
                Response(operSide)
            ).ToList();
        }

        public async Task<OperationSideResponse?> GetByIdsAsync(int operationId, int warSideId)
        {
            var operationSides = await _repository
                .GetByIdsAsync(operationId, warSideId);
            if (operationSides == null)
                return null;

            return Response(operationSides);
        }

        public async Task<List<OperationSideResponse>> GetByOperationIdAsync(int operationId)
        {
            var operations = await _repository
                .GetByOperationIdAsync(operationId);

            return operations.Select(oper => 
                Response(oper)
                ).ToList();
        }

        public async Task<List<OperationSideResponse>> GetByWarSideIdAsync(int warSideId)
        {
            var warSides = await _repository
                .GetByWarSideIdAsync(warSideId);

            return warSides.Select(warSide => 
                Response(warSide)
                ).ToList();
        }

        public async Task AddAsync(CreateOperationSideRequest operationSideRequest)
        {
            var opS = operationSideRequest;
            var operSide = new OperationSide
            {
                operation_id = opS.OperationId,
                war_side_id = opS.WarSideId,
                role_side = opS.RoleSide,
                note = opS.Note
            };

            await _repository.AddAsync(operSide);
        }

        public async Task UpdateAsync(int operationId, int warSideId, UpdateOperationSideRequest operationSideRequest)
        {
            var operSideReq = operationSideRequest;
            var operationSide = await _repository
                .GetByIdsAsync(operationId, warSideId);

            if (operationSide == null)
                Exception();

            operationSide.role_side = operSideReq.RoleSide;
            operationSide.note = operSideReq.Note;

            await _repository.UpdateAsync(operationSide);
        }

        public async Task DeleteAsync(int operationId, int warSideId)
        {
            var operationSide = await _repository
                    .GetByIdsAsync(operationId, warSideId);
            if (operationSide == null)
                Exception();

            await _repository.DeleteAsync(operationId, warSideId);
        }

        private OperationSideResponse Response(OperationSide operationSide) =>
            new OperationSideResponse(
                operationSide.operation_id,
                operationSide.war_side_id,
                operationSide.role_side,
                operationSide.note
            );

        private void Exception() =>
            throw new KeyNotFoundException("operation side not found");
    }
}
