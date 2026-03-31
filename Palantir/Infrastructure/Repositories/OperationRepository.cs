using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class OperationRepository : IOperationRepository
    {
        private readonly PalantirDbContext _dbContext;

        public OperationRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Operation>> GetAllAsync() =>
            await _dbContext.operations.ToListAsync<Operation>();

        public async Task<Operation?> GetByIdAsync(int id) =>
            await _dbContext.operations.FirstOrDefaultAsync(o => o.operation_id == id);

    }
}
