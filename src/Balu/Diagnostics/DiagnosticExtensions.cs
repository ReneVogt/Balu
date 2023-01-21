using System.Collections.Generic;
using System.Linq;

namespace Balu.Diagnostics;

public static class DiagnosticExtensions
{
    public static bool HasErrors(this IEnumerable<Diagnostic> diagnostics) => diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}