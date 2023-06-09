using Microsoft.CodeAnalysis;
using Nox.Solution;
using System.Text;

namespace Nox.Generator.Generators
{
    internal class DtoGenerator : BaseGenerator
    {
        internal DtoGenerator(GeneratorExecutionContext context) : base(context) { }

        internal void AddDTO(DataTransferObject dto)
        {
            var sb = new StringBuilder();

            var className = $"{dto.Name}{NamingConstants.DtoSuffix}";

            AddBaseTypeDefinition(sb,
                className,
                "IDynamicDto",
                "Nox.Dto",
                isAbstract: false,
                isPartial: false,
                "Nox.Core.Interfaces.Entity");

            // Attributes
            AddAttributes(dto.Attributes, sb);

            sb.AppendLine($@"}}");

            GenerateFile(sb, className);
        }
    }
}
