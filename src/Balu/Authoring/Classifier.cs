using System;
using System.Collections.Immutable;
using Balu.Syntax;
using Balu.Text;

namespace Balu.Authoring;

public static class Classifier
{
    public static ImmutableArray<ClassifiedSpan> Classify(SyntaxTree syntaxTree, TextSpan span)
    {
        _ = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
        var resultBuilder = ImmutableArray.CreateBuilder<ClassifiedSpan>();
        ClassifyNode(syntaxTree.Root, span, resultBuilder);
        return resultBuilder.ToImmutable();
    }

    static void ClassifyNode(SyntaxNode node, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder resultBuilder)
    {
        if (!node.FullSpan.OverlapsWith(span)) return;
        if (node is SyntaxToken token)
            ClassifyToken(token, span, resultBuilder);

        foreach (var child in node.Children)
            ClassifyNode(child, span, resultBuilder);
    }
    static void ClassifyToken(SyntaxToken token, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder resultBuilder)
    {
        foreach (var trivia in token.LeadingTrivia)
            AddClassifiedSpan(trivia.Kind, trivia.Span, span, resultBuilder);

        AddClassifiedSpan(token.Kind, token.Span, span, resultBuilder);

        foreach (var trivia in token.TrailingTrivia)
            AddClassifiedSpan(trivia.Kind, trivia.Span, span, resultBuilder);
    }

    static void AddClassifiedSpan(SyntaxKind kind, TextSpan elementSpan, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder resultBuilder)
    {
        if (!elementSpan.OverlapsWith(span)) return;
        var classififcation = kind.IsKeyword()
                                  ? Classification.Keyword
                                  : kind.IsComment()
                                      ? Classification.Comment
                                      : kind switch
                                      {
                                          SyntaxKind.IdentifierToken => Classification.Identifier,
                                          SyntaxKind.NumberToken => Classification.Number,
                                          SyntaxKind.StringToken => Classification.String,
                                          SyntaxKind.BadToken or SyntaxKind.SkippedTextTrivia => Classification.Bad,
                                          _ => Classification.Text
                                      };

        var start = Math.Max(elementSpan.Start, span.Start);
        var end = Math.Min(elementSpan.End, span.End);
        resultBuilder.Add(new(new(start, end-start), classififcation));
    }
}