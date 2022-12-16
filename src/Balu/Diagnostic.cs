using Balu.Text;

namespace Balu;

/// <summary>
/// Represents a Balu compilation error message.
/// </summary>
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


    /// <summary>
    /// An id to identify the kind of error message.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The span in the input Balu string that causes this error.
    /// </summary>
    public TextSpan Span { get; }

    /// <summary>
    /// The specific error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new <see cref="Diagnostic"/> instance.
    /// </summary>
    /// <param name="id">An id to identify the kind of error message.</param>
    /// <param name="span"><inheritdoc cref="Span"/></param>
    /// <param name="message"><inheritdoc cref="Message"/></param>
    public Diagnostic(string id, TextSpan span, string message) => (Id, Span, Message) = (id, span, message);

    /// <inheritdoc />
    public override string ToString() => $"[{Id}]{Span}: {Message}";
}
