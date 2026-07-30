using Balu.Diagnostics;
using Balu.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Balu.Syntax;

public sealed class SyntaxTree
{
    delegate void ParseHandler(SyntaxTree syntaxTree, CancellationToken cancellationToken, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics);

    Dictionary<SyntaxNode, SyntaxNode?>? parents;

    public CompilationUnitSyntax Root { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public SourceText Text { get; }

    public bool IsLastTokenMissing => Root.Members.LastOrDefault()?.LastToken.IsMissing ?? true;

    SyntaxTree(SourceText text, ParseHandler parser, CancellationToken cancellationToken)
    {
        Text = text;
        cancellationToken.ThrowIfCancellationRequested();
        parser(this, cancellationToken, out var root, out var diagnostics);
        Root = root;
        Diagnostics = diagnostics;
    }

    internal SyntaxNode? GetParent(SyntaxNode syntaxNode)
    {
        if (parents == null)
        {
            var created = CreateParentsDictionary(Root);
            Interlocked.CompareExchange(ref parents, created, null);
        }

        return parents[syntaxNode];
    }
    static Dictionary<SyntaxNode, SyntaxNode?> CreateParentsDictionary(CompilationUnitSyntax root)
    {
        var result = new Dictionary<SyntaxNode, SyntaxNode?> { { root, null } };
        CreateParentsDictionary(result, root);
        return result;
    }
    static void CreateParentsDictionary(Dictionary<SyntaxNode, SyntaxNode?> result, SyntaxNode node)
    {
        for (int i = 0; i < node.ChildrenCount; i++)
        {
            var child = node.GetChild(i);
            result.Add(child, node);
            CreateParentsDictionary(result, child);
        }
    }

    public static SyntaxTree Load(string fileName, CancellationToken cancellationToken = default) => Parse(SourceText.Load(fileName, cancellationToken), cancellationToken);

    public static SyntaxTree Parse(string input, CancellationToken cancellationToken = default) => Parse(SourceText.From(input, cancellationToken: cancellationToken), cancellationToken);
    public static SyntaxTree Parse(SourceText text, CancellationToken cancellationToken = default) => new(text, Parse, cancellationToken);

    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens, CancellationToken cancellationToken = default) =>
        ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens)), cancellationToken: cancellationToken), cancellationToken);
    public static ImmutableArray<SyntaxToken> ParseTokens(string tokens, out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default) =>
        ParseTokens(SourceText.From(tokens ?? throw new ArgumentNullException(nameof(tokens)), cancellationToken: cancellationToken), out diagnostics, cancellationToken);
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source, CancellationToken cancellationToken = default) => ParseTokens(source, out _, cancellationToken);
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableArray.CreateBuilder<SyntaxToken>();
        void TokenParser(SyntaxTree syntaxTree, CancellationToken cancellationToken, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
        {
            var lexer = new Lexer(syntaxTree, cancellationToken);
            using var enumerator = lexer.Lex().GetEnumerator();
            while (enumerator.MoveNext() && enumerator.Current!.Kind != SyntaxKind.EndOfFileToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.Add(enumerator.Current);
            }

            root = new(syntaxTree, ImmutableArray<MemberSyntax>.Empty, enumerator.Current!);
            diagnostics = lexer.Diagnostics.ToImmutableArray();
        }

        var syntaxTree = new SyntaxTree(source, TokenParser, cancellationToken);
        diagnostics = syntaxTree.Diagnostics;
        return builder.ToImmutableArray();
    }

    static void Parse(SyntaxTree syntaxTree, CancellationToken cancellationToken, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
    {
        var parser = new Parser(syntaxTree, cancellationToken);
        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }
}
