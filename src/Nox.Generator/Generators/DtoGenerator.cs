using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class DtoGenerator : BaseGenerator
    {
        internal DtoGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddDTO(Dictionary<object, object> dto)
        {
            var sb = new StringBuilder();

            var className = $"{dto["name"]}{NamingConstants.DtoSuffix}";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicDTO",
                "Nox.Dto",
                isAbstract: false,
                "Nox.Core.Interfaces.Entity");
            
            AddAttributes(dto, sb);

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
