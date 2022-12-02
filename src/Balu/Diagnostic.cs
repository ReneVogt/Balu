using Balu.Syntax;
using System;

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

    internal static Diagnostic LexerUnexpectedToken(int start, int length, string text) =>
        new ("LEX0000", new (start, length), $"Unexpected token '{text}'.");
    internal static Diagnostic LexerNumberNotValid(int start, int length, string text) =>
        new ("LEX0001", new (start, length), $"The number '{text}' is not a valid 32bit integer.");

    internal static Diagnostic ParserUnexpectedToken(TextSpan textSpan, SyntaxToken foundToken, SyntaxKind expected) =>
        new ("SYX0000", textSpan, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}.");

    internal static Diagnostic BinderUnaryOperatorTypeMismatch(string operatorText, Type type) =>
        new("BND0000", default, $"Unary operator '{operatorText}' cannot be applied to type '{type}'.");
    internal static Diagnostic BinderBinaryOperatorTypeMismatch(string operatorText, Type left, Type right) =>
        new("BND0001", default, $"Binary operator '{operatorText}' cannot be applied to types '{left}' and '{right}'.");

}
