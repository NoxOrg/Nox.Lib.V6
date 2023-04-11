using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class QueryGenerator : BaseGenerator
    {
        internal QueryGenerator(GeneratorExecutionContext context) 
            : base(context) { }

        internal void AddQuery(Dictionary<object, object> query)
        {
            var sb = new StringBuilder();

            var className = $"{query["name"]}Query";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicQuery",
                "Nox.Queries",
                isAbstract: true,
                new[] 
                { 
                    "Nox.Core.Interfaces.Entity",
                    "Nox.Dto"
                });

            // Add Db Context
            AddDbContextProperty(sb);

            // Add constructor
            AddConstructor(sb, className, new Dictionary<string, string> {
                { "NoxDomainDbContext", "DbContext" }
            });

            // Add params (which can be DTO)
            string parameters = GetParametersString(query["parameters"]);

            var response = (Dictionary<object, object>)query["response"];

            bool isMany = bool.Parse((string)response["isMany"]);
            var dto = response["responseDto"];

            var typeDefinition = isMany ? $"IList<{dto}>" : $"{dto}";
            
            sb.AppendLine($@"   public abstract Task<{typeDefinition}> ExecuteAsync({parameters});");

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
