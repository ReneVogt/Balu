using Balu.Text;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// A syntax tree representing a Balu code file.
/// </summary>
public sealed class SyntaxTree
{
    /// <summary>
    /// The root of the syntax tree.
    /// </summary>
    public CompilationUnitSyntax Root { get; }
    /// <summary>
    /// The error messages collected during compilation.
    /// </summary>
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    /// <summary>
    /// The <see cref="Text"/> representing the original source text.
    /// </summary>
    public SourceText Text { get; }

    /// <summary>
    /// Returns if the last token of this tree
    /// was actually missing in the original source code.
    /// </summary>
    public bool IsLastTokenMissing => Root.Members.Last().LastToken.IsMissing;

    SyntaxTree(SourceText text)
    {
        Text = text;
        var parser = new Parser(text);
        Root = parser.ParseCompilationUnit();
        Diagnostics = parser.Diagnostics.ToImmutableArray();
    }

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
    /// <param name="text">The Balu code to parse.</param>
    /// <returns>The <see cref="SyntaxTree"/> representing the parsed code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
    public static SyntaxTree Parse(SourceText text) => new (text);

    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="tokens">The input string to parse.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tokens"/> is <c>null</c>.</exception>
    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))));
    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="tokens">The input string to parse.</param>
    /// <param name="diagnostics">Receives the <see cref="ImmutableArray{Diagnostic}"/> with parsing error messages.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tokens"/> is <c>null</c>.</exception>
    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens, out ImmutableArray<Diagnostic> diagnostics) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))), out diagnostics);

    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="source">The input <see cref="Text"/> to parse.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source) => ParseTokens(source, out _);
    /// <summary>
    /// Parses an input string into a sequence of Balu <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="source">The input <see cref="Text"/> to parse.</param>
    /// <param name="diagnostics">Receives the <see cref="ImmutableArray{Diagnostic}"/> with parsing error messages.</param>
    /// <returns>A sequence of <see cref="SyntaxToken"/> representing the input code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics)
    {
        var lexer = new Lexer(source ?? throw new ArgumentNullException(nameof(source)));
        var tokens = lexer.Lex().TakeWhile(token => token.Kind != SyntaxKind.EndOfFileToken).ToImmutableArray();
        diagnostics = lexer.Diagnostics.ToImmutableArray();
        return tokens;
    }
}
