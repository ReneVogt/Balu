﻿using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class CallExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.CallExpression;
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return Identifier;
            yield return OpenParenthesis;
            foreach (var node in Arguments.ElementsWithSeparators)
                yield return node;
            yield return ClosedParenthesis;
        }
    }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken ClosedParenthesis { get; }

    public CallExpressionSyntax(SyntaxTree? syntaxTree, SyntaxToken identifier, SyntaxToken openParenthesis,
                                SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closedParenthesis)
        : base(syntaxTree)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        OpenParenthesis = openParenthesis ?? throw new ArgumentNullException(nameof(openParenthesis));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        ClosedParenthesis = closedParenthesis ?? throw new ArgumentNullException(nameof(closedParenthesis));
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var identifier = (SyntaxToken)visitor.Visit(Identifier);
        var open = (SyntaxToken)visitor.Visit(OpenParenthesis);

        var arguments = VisitList(visitor, Arguments);
        var close = (SyntaxToken)visitor.Visit(ClosedParenthesis);
        return identifier == Identifier && open == OpenParenthesis && arguments == Arguments &&
               close == ClosedParenthesis
                   ? this
                   : new(null, identifier, open, arguments, close);
    }

    public override string ToString() => $"{Kind}{Span}: {Identifier.Text}";
}