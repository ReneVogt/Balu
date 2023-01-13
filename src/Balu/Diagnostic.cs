using Balu.Text;

namespace Balu;

public sealed class Diagnostic
{
    public string Id { get; }
    public TextLocation Location { get; }
    public string Message { get; }
    
    public Diagnostic(string id, TextLocation location, string message) => (Id, Location, Message) = (id, location, message);

    public override string ToString() => $"{Location}: [{Id}] {Message}";
}
