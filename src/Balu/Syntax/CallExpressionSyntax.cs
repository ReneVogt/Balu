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

        var arguments = VisitList(visitor, Arguments);
        var close = (SyntaxToken)visitor.Visit(ClosedParenthesis);
        return identifier == Identifier && open == OpenParenthesis && arguments == Arguments &&
               close == ClosedParenthesis
                   ? this
                   : Call(identifier, open, arguments, close);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}{Span}: {Identifier.Text}";
}