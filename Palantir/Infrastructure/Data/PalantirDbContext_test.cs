using Microsoft.EntityFrameworkCore;
using Palantir.Domain.Models;
using System.Data.Common;

namespace Palantir.Infrastructure.Data
{
    public class PalantirDbContext_test : DbContext
    {
        public PalantirDbContext_test(DbContextOptions<PalantirDbContext_test> options) : base(options)
        {
        }
    }
}
