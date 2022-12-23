using Balu.Text;

namespace Balu;

public sealed class Diagnostic
{
    public const string LEX0000 = nameof(LEX0000);
    public const string LEX0001 = nameof(LEX0001);
    public const string LEX0002 = nameof(LEX0002);
    public const string LEX0003 = nameof(LEX0003);

    public const string SYX0000 = nameof(SYX0000);
    
    public const string BND0000 = nameof(BND0000);
    public const string BND0001 = nameof(BND0001);
    public const string BND0002 = nameof(BND0002);
    public const string BND0003 = nameof(BND0003);
    public const string BND0004 = nameof(BND0004);
    public const string BND0005 = nameof(BND0005);
    public const string BND0006 = nameof(BND0006);
    public const string BND0007 = nameof(BND0007);
    public const string BND0008 = nameof(BND0008);
    public const string BND0009 = nameof(BND0009);
    public const string BND0010 = nameof(BND0010);
    public const string BND0011 = nameof(BND0011);
    public const string BND0012 = nameof(BND0012);
    public const string BND0013 = nameof(BND0013);
    public const string BND0014 = nameof(BND0014);
    public const string BND0015 = nameof(BND0015);
    public const string BND0016 = nameof(BND0016);
    public const string BND0017 = nameof(BND0017);
    public const string BND0018 = nameof(BND0018);
    public const string BND0019 = nameof(BND0019);
    public const string BND0020 = nameof(BND0020);

    public string Id { get; }
    public TextLocation Location { get; }
    public string Message { get; }
    
    public Diagnostic(string id, TextLocation location, string message) => (Id, Location, Message) = (id, location, message);

    public override string ToString() => $"{Location}: [{Id}] {Message}";
}
