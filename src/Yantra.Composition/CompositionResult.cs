using Yantra.Domain;

namespace Yantra.Composition;

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public sealed record CompositionDiagnostic(DiagnosticSeverity Severity, string Message);

public sealed record CompositionResult(
    SystemDefinition? System,
    IReadOnlyList<CompositionDiagnostic> Diagnostics)
{
    public bool IsSuccess => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
}
