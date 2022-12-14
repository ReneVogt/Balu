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
            foreach (var node in Parameters.ElementsWithSeparators)
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
    public SeparatedSyntaxList<ExpressionSyntax> Parameters { get; }
    /// <summary>
    /// The closed parenthesis of the call expression.
    /// </summary>
    public SyntaxToken ClosedParenthesis { get; }

    internal CallExpressionSyntax(SyntaxToken identifier, SyntaxToken openParenthesis, SeparatedSyntaxList<ExpressionSyntax> parameters, SyntaxToken closedParenthesis)
    {
        Identifier = identifier;
        OpenParenthesis = openParenthesis;
        Parameters = parameters;
        ClosedParenthesis = closedParenthesis;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var identifier = (SyntaxToken)visitor.Visit(Identifier);
        var open = (SyntaxToken)visitor.Visit(OpenParenthesis);
        List<SyntaxNode>? p = null;
        
        for (int i=0; i<Parameters.ElementsWithSeparators.Length; i++)
        {
            var node = visitor.Visit(Parameters.ElementsWithSeparators[i]);
            if (p is not null)
            {
                p.Add(node);
                continue;
            }

            if (node == Parameters.ElementsWithSeparators[i]) continue;
            p = Parameters.ElementsWithSeparators.Take(i).ToList();
        }
        var close = (SyntaxToken)visitor.Visit(ClosedParenthesis);
        return identifier == Identifier && open == OpenParenthesis && p is null &&
               close == ClosedParenthesis
                   ? this
                   : Call(identifier, open, p is null ? Parameters : new (p), close);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}{Span}: {Identifier.Text}";
}