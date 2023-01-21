using Balu.Text;

namespace Balu.Diagnostics;

public sealed class Diagnostic
{
    public DiagnosticId Id { get; }
    public string IdString { get; }
    public DiagnosticSeverity Severity { get; }
    public TextLocation Location { get; }
    public string Message { get; }

    internal Diagnostic(DiagnosticId id, TextLocation location, string message, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        Id = id;
        IdString = $"BL{(int)id:0000}";
        Location = location;
        Message = message;
        Severity = severity;
    }

    public override string ToString() => $"{Location}: [{IdString}] {Message}";
}
