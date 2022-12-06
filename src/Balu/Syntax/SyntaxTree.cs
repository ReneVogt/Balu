using Balu.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// A syntax tree representing a Balu code file.
/// </summary>
public sealed class SyntaxTree
{
    /// <summary>
    /// The root expression of the syntax tree.
    /// </summary>
    public ExpressionSyntax Root { get; }
    /// <summary>
    /// The eof token of the parsed input.
    /// </summary>
    public SyntaxToken EndOfFileToken { get; }
    /// <summary>
    /// The error messages collected during compilation.
    /// </summary>
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    /// <summary>
    /// The <see cref="Text"/> representing the original source text.
    /// </summary>
    public SourceText Text { get; }

    internal SyntaxTree(SourceText text, ExpressionSyntax root, SyntaxToken endOfFileToken, IEnumerable<Diagnostic> diagnostics) =>
        (Text, Root, EndOfFileToken, Diagnostics) = (text, root, endOfFileToken, diagnostics.ToImmutableArray());

    /// <summary>
    /// Parses a input string into a Balu syntax tree.
    /// </summary>
    /// <param name="input">The Balu code to parse.</param>
    /// <returns>The <see cref="SyntaxTree"/> representing the parsed code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    public static SyntaxTree Parse(string input) => Parse(SourceText.From(input));

    /// <summary>
    /// Parses a input string into a Balu syntax tree.
    /// </summary>
    /// <param name="input">The Balu code to parse.</param>
    /// <returns>The <see cref="SyntaxTree"/> representing the parsed code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    public static SyntaxTree Parse(SourceText input) => new Parser(input ?? throw new ArgumentNullException(nameof(input))).Parse();

    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="tokens">The input string to parse.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tokens"/> is <c>null</c>.</exception>
    public static IEnumerable<SyntaxToken> ParseTokens(string tokens) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))));

    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="source">The input <see cref="Text"/> to parse.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static IEnumerable<SyntaxToken> ParseTokens(SourceText source) =>
        new Lexer(source ?? throw new ArgumentNullException(nameof(source))).Lex().TakeWhile(token => token.Kind != SyntaxKind.EndOfFileToken);

}
