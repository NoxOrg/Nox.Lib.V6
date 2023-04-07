using ETLBox.Connection;
using ETLBox.DataFlow;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nox.Core.Interfaces.Etl;
using Nox.Core.Models;
using SqlKata.Compilers;
using System.Dynamic;

namespace Nox.Core.Interfaces.Database
{
    public interface IDataProvider
    {
        string Name { get; }
        string ConnectionString { get; set; }
        IConnectionManager ConnectionManager { get; }
        Compiler SqlCompiler { get; }

        void Configure(IServiceDataSource serviceDb, string applicationName);
        DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);
        EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema);
        IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration);

        string ToDatabaseColumnType(BaseEntityAttribute entityAttribute);
        string ToTableNameForSql(string table, string schema);
        string ToTableNameForSqlRaw(string table, string schema);

        IDataFlowExecutableSource<ExpandoObject> DataFlowSource(ILoaderSource loaderSource);
        void ApplyMergeInfo(ILoaderSource loaderSource, LoaderMergeStates lastMergeDateTimeStampInfo, string[] dateTimeStampColumns, string[] targetColumns);
    }
}