using ETLBox.Connection;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Nox.Dynamic.MetaData;
using SqlKata.Compilers;

namespace Nox.Dynamic.DatabaseProviders
{
    public interface IDatabaseProvider
    {
        public string ConnectionString { get; }

        public IConnectionManager ConnectionManager { get; }

        public Compiler SqlCompiler { get; }

        public void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);

        public string ToTableNameForSql(Entity entity);

        public string ToDatabaseColumnType(EntityAttribute entityAttribute);

        public IGlobalConfiguration ConfigureHangfire(IGlobalConfiguration configuration);

    }
}