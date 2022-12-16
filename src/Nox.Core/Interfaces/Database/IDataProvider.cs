using ETLBox.Connection;
using ETLBox.DataFlow;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Interfaces.Entity;
using Nox.Core.Interfaces.Etl;
using SqlKata.Compilers;
using System.Dynamic;

namespace Nox.Core.Interfaces.Database
{
    public interface IDataProvider
    {
        public string Name { get; }
        public string ConnectionString { get; }
        public IDataFlowExecutableSource<ExpandoObject> DataFlowSource(ILoaderSource loaderSource);
        public IConnectionManager ConnectionManager { get; }
        public Compiler SqlCompiler { get; }
        public void ConfigureServiceDatabase(IServiceDataSource serviceDb, string applicationName);
        public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);
        public IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration);
        public string ToDatabaseColumnType(IEntityAttribute entityAttribute);
        string ToTableNameForSql(string table, string schema);
        string ToTableNameForSqlRaw(string table, string schema);
        EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema);

    }
}