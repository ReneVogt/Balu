using System;
using System.Collections.Generic;
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
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    internal SyntaxTree(ExpressionSyntax root, SyntaxToken endOfFileToken, IEnumerable<Diagnostic> diagnostics) =>
        (Root, EndOfFileToken, Diagnostics) = (root, endOfFileToken, diagnostics.ToArray());

    /// <summary>
    /// Parses a input string into a Balu syntax tree.
    /// </summary>
    /// <param name="input">The Balu code to parse.</param>
    /// <returns>The <see cref="SyntaxTree"/> representing the parsed code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    public static SyntaxTree Parse(string input) => new Parser(input ?? throw new ArgumentNullException(nameof(input))).Parse();

    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="tokens">The input string to parse.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tokens"/> is <c>null</c>.</exception>
    public static IEnumerable<SyntaxToken> ParseTokens(string tokens) =>
        new Lexer(tokens ?? throw new ArgumentNullException(nameof(tokens))).Lex().TakeWhile(token => token.Kind != SyntaxKind.EndOfFileToken);
    
}
