namespace Balu;

/// <summary>
/// Represents a Balu compilation error message.
/// </summary>
public sealed class Diagnostic
{
    /// <summary>
    /// An id to identify the kind of error message.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The span in the input Balu string that causes this error.
    /// </summary>
    public TextSpan TextSpan { get; }

    /// <summary>
    /// The specific error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new <see cref="Diagnostic"/> instance.
    /// </summary>
    /// <param name="id">An id to identify the kind of error message.</param>
    /// <param name="textSpan"><inheritdoc cref="TextSpan"/></param>
    /// <param name="message"><inheritdoc cref="Message"/></param>
    public Diagnostic(string id, TextSpan textSpan, string message) => (Id, TextSpan, Message) = (id, textSpan, message);

    /// <inheritdoc />
    public override string ToString() => $"[{Id}]{TextSpan}: {Message}";
}
