using Microsoft.CodeAnalysis;

namespace Nox.Generator
{
    internal static class WarningsErrors
    {
        internal static readonly DiagnosticDescriptor NI0000 = new ("NI0000", "Nox Generator Debug",
            "{0}", "Debug", DiagnosticSeverity.Info, true);

        #region Warnings

        internal static readonly DiagnosticDescriptor NW0001 = new("NW0001", "No yaml definitions",
            "Nox.Generator will not contribute to your project as no yaml definitions were found", "Design",
            DiagnosticSeverity.Warning, true);

        internal static readonly DiagnosticDescriptor NW0002 = new("NW0002", "AppSettings",
            "DefinitionRootPath value not found in appsettings.json", "Design",
            DiagnosticSeverity.Warning, true);

        #endregion Warnings

        #region Errors

        internal static readonly DiagnosticDescriptor NE0000 = new(
            "NE0000",
            "Generation Error",
            "Generation Error: {0}",
            "Design",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor NE0001 = new("NE0001", "Duplicate Entity",
            "Duplicate entity detected in yaml configuration: {0}", "Design",
            DiagnosticSeverity.Error, true);

        #endregion Errors

        public static void ReportDiagnosticsDebug(this GeneratorExecutionContext context, string message)
        {
            context.ReportDiagnostic(Diagnostic.Create(NI0000, null, message));
        }

        public static void ReportDiagnosticsError(this GeneratorExecutionContext context, string message)
        {
            context.ReportDiagnostic(Diagnostic.Create(NE0000, null, message));
        }
    }
}