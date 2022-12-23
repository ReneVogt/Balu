using Balu.Text;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Balu.Syntax;

public sealed class SyntaxTree
{
    public CompilationUnitSyntax Root { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public SourceText Text { get; }

    public bool IsLastTokenMissing => Root.Members.Last().LastToken.IsMissing;

    SyntaxTree(SourceText text)
    {
        Text = text;
        var parser = new Parser(this);
        Root = parser.ParseCompilationUnit();
        Diagnostics = parser.Diagnostics.ToImmutableArray();
    }

    public static SyntaxTree Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(string input) => Parse(SourceText.From(input));
    public static SyntaxTree Parse(SourceText text) => new(text);

    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))));
    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens, out ImmutableArray<Diagnostic> diagnostics) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))), out diagnostics);
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source) => ParseTokens(source, out _);
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics)
    {
        var builder = ImmutableArray.CreateBuilder<SyntaxToken>();
        var lexer = new Lexer(source);
        builder.AddRange(lexer.Lex().TakeWhile(token => token.Kind != SyntaxKind.EndOfFileToken));
        diagnostics = lexer.Diagnostics.ToImmutableArray();
        return builder.ToImmutableArray();
    }
}
