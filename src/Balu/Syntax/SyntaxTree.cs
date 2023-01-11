using Balu.Text;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Balu.Syntax;

public sealed class SyntaxTree
{
    delegate void ParseHandler(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics);

    public CompilationUnitSyntax Root { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public SourceText Text { get; }

    public bool IsLastTokenMissing => Root.Members.LastOrDefault()?.LastToken.IsMissing ?? false;

    SyntaxTree(SourceText text, ParseHandler parser)
    {
        Text = text;
        parser(this, out var root, out var diagnostics);
        Root = root;
        Diagnostics = diagnostics;
    }

    public static SyntaxTree Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(string input) => Parse(SourceText.From(input));
    public static SyntaxTree Parse(SourceText text) => new(text, Parse);

    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))));
    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens, out ImmutableArray<Diagnostic> diagnostics) => ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens))), out diagnostics);
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source) => ParseTokens(source, out _);
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics)
    {
        var builder = ImmutableArray.CreateBuilder<SyntaxToken>();
        void TokenParser(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
        {
            var lexer = new Lexer(syntaxTree);
            using var enumerator = lexer.Lex().GetEnumerator();
            while (enumerator.MoveNext() && enumerator.Current.Kind != SyntaxKind.EndOfFileToken)
                builder.Add(enumerator.Current);

            root = new(syntaxTree, ImmutableArray<MemberSyntax>.Empty, enumerator.Current);
            diagnostics = lexer.Diagnostics.ToImmutableArray();
        }

        var syntaxTree = new SyntaxTree(source, TokenParser);
        diagnostics = syntaxTree.Diagnostics;
        return builder.ToImmutableArray();
    }

    static void Parse(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
    {
        var parser = new Parser(syntaxTree);
        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }
}
