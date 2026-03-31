using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class OperationSideRepository : IOperationSideRepository
    {
        private readonly PalantirDbContext _dbContext;

        public OperationSideRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OperationSide>> GetAllAsync() =>
            await _dbContext.operation_sides.ToListAsync<OperationSide>();

    }
}
