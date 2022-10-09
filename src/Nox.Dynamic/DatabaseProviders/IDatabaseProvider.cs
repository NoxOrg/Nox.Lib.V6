using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nox.Dynamic.MetaData;

namespace Nox.Dynamic.DatabaseProviders
{
    public interface IDatabaseProvider
    {
        public string ConnectionString { get; }

        public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);

        public string ToDatabaseColumnType(EntityAttribute entityAttribute);

        public Task<bool> LoadData(Service service, ILogger logger);
    }
}