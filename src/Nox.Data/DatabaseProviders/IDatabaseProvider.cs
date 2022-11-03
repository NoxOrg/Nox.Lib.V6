using ETLBox.Connection;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SqlKata.Compilers;

namespace Nox.Data
{
    public interface IDatabaseProvider
    {
        public string Name { get; }
        public string ConnectionString { get; }
        public IConnectionManager ConnectionManager { get; }
        public Compiler SqlCompiler { get; }
        public void ConfigureServiceDatabase(IServiceDatabase serviceDb, string applicationName);
        public DbContextOptionsBuilder ConfigureDbContext(DbContextOptionsBuilder optionsBuilder);
        public IGlobalConfiguration ConfigureJobScheduler(IGlobalConfiguration configuration);
        public string ToDatabaseColumnType(IEntityAttribute entityAttribute);
        string ToTableNameForSql(string table, string schema);
        string ToTableNameForSqlRaw(string table, string schema);
        EntityTypeBuilder ConfigureEntityTypeBuilder(EntityTypeBuilder builder, string table, string schema);

    }
}