using Microsoft.CodeAnalysis;
using Nox.Solution;
using Nox.Types;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class QueryGenerator : BaseGenerator
    {
        internal QueryGenerator(GeneratorExecutionContext context)
            : base(context) { }

        internal void AddQuery(DomainQuery query)
        {
            var sb = new StringBuilder();

            var className = $"{query.Name}Query";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicQuery",
                "Nox.Queries",
                isAbstract: true,
                isPartial: false,
                    "Nox.Core.Interfaces.Entity",
                    "Nox.Dto"
                );

            // Add Db Context
            AddDbContextProperty(sb);

            // Add constructor
            AddConstructor(sb, className, new Dictionary<string, string> {
                { NamingConstants.DbContextName, "DbContext" }
            });

            // Add params (which can be DTO)
            string parameters = GetParametersString(query.RequestInput);

            bool isMany = query.ResponseOutput.Type == NoxType.Array || query.ResponseOutput.Type == NoxType.Collection;
            var dto = query.ResponseOutput.Name;

            var typeDefinition = isMany ? $"IEnumerable<{dto}>" : $"{dto}";

            sb.AppendLine($@"   public abstract Task<{typeDefinition}> ExecuteAsync({parameters});");

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
