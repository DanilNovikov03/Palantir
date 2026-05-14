using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Palantir.Application.Services
{
    public class OperationSideService : IOperationSideService
    {
        IOperationSideRepository _repository;

        public OperationSideService(IOperationSideRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OperationSideResponse>> GetAllAsync()
        {
            var operSides =
                await _repository.GetAllAsync();

            return operSides.Select(operSide => new OperationSideResponse(
                operSide.operation_id,
                operSide.war_side_id,
                operSide.role_side,
                operSide.note
            )).ToList();
        }

        public async Task<OperationSideResponse?> GetByIdsAsync(int operationId, int warSideId)
        {
            var operationSides = await _repository
                .GetByIdsAsync(operationId, warSideId);
            if (operationSides == null)
                return null;

            return new OperationSideResponse(
                operationSides.operation_id,
                operationSides.war_side_id,
                operationSides.role_side,
                operationSides.note
            );
        }

        public async Task<List<OperationSideResponse>> GetByOperationIdAsync(int operationId)
        {
            var operations = await _repository
                .GetByOperationIdAsync(operationId);

            return operations.Select(oper => new OperationSideResponse(
                oper.operation_id,
                oper.war_side_id,
                oper.role_side,
                oper.note
            )).ToList();
        }

        public async Task<List<OperationSideResponse>> GetByWarSideIdAsync(int warSideId)
        {
            var warSides = await _repository
                .GetByWarSideIdAsync(warSideId);

            return warSides.Select(warSide => new OperationSideResponse(
                warSide.operation_id,
                warSide.war_side_id,
                warSide.role_side,
                warSide.note
            )).ToList();
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

        private void Exception() =>
            throw new KeyNotFoundException("operation side not found");
    }
}
