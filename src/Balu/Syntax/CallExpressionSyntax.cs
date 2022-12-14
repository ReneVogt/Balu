using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// The <see cref="SyntaxToken"/> identifying the name of the function.
    /// </summary>
    public SyntaxToken Identifier { get; }
    /// <summary>
    /// The open parenthesis of the call expression.
    /// </summary>
    public SyntaxToken OpenParenthesis { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    /// <summary>
    /// The closed parenthesis of the call expression.
    /// </summary>
    public SyntaxToken ClosedParenthesis { get; }

    internal CallExpressionSyntax(SyntaxToken identifier, SyntaxToken openParenthesis, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closedParenthesis)
    {
        Identifier = identifier;
        OpenParenthesis = openParenthesis;
        Arguments = arguments;
        ClosedParenthesis = closedParenthesis;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var identifier = (SyntaxToken)visitor.Visit(Identifier);
        var open = (SyntaxToken)visitor.Visit(OpenParenthesis);
        List<SyntaxNode>? p = null;
        
        for (int i=0; i<Arguments.ElementsWithSeparators.Length; i++)
        {
            var node = visitor.Visit(Arguments.ElementsWithSeparators[i]);
            if (p is not null)
            {
                p.Add(node);
                continue;
            }

            if (node == Arguments.ElementsWithSeparators[i]) continue;
            p = Arguments.ElementsWithSeparators.Take(i+1).ToList();
        }
        var close = (SyntaxToken)visitor.Visit(ClosedParenthesis);
        return identifier == Identifier && open == OpenParenthesis && p is null &&
               close == ClosedParenthesis
                   ? this
                   : Call(identifier, open, p is null ? Arguments : new (p), close);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}{Span}: {Identifier.Text}";
}