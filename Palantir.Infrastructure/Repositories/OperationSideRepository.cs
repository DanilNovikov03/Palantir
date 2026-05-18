namespace Palantir.Infrastructure.Repositories
{
    public class OperationSideRepository : IOperationSideRepository
    {
        private readonly PalantirDbContext _dbContext;

        public OperationSideRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;


        public async Task<List<OperationSide>> GetByOperationIdAsync(int operationId) =>
            await _dbContext.operation_sides
                .Where(op => op.operation_id == operationId)
                .ToListAsync();

        public async Task<List<OperationSide>> GetByWarSideIdAsync(int warSideId) =>
            await _dbContext.operation_sides
                .Where(ws => ws.war_side_id == warSideId)
                .ToListAsync();

        public async Task<OperationSide?> GetByIdsAsync(int operationId, int warSideId) =>
            await _dbContext.operation_sides
                .FirstOrDefaultAsync(opS => 
                    opS.operation_id == operationId && 
                    opS.war_side_id == warSideId
                );

        public async Task<List<OperationSide>> GetAllAsync() =>
            await _dbContext
                .operation_sides.ToListAsync<OperationSide>();

        public async Task AddAsync(OperationSide operationSide)
        {
            await _dbContext
                .operation_sides.AddAsync(operationSide);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(OperationSide operationSide)
        {
            var existOperSide = await _dbContext.operation_sides
                .FindAsync(operationSide.operation_id, operationSide.war_side_id);

            if (existOperSide == null)
                return;

            existOperSide.role_side = operationSide.role_side;
            existOperSide.note = operationSide.note;

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int operationId, int warSideId)
        {
            var operSide =
                await _dbContext.operation_sides
                    .FindAsync(operationId, warSideId);

            if (operSide != null)
            {
                _dbContext.operation_sides.Remove(operSide);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
