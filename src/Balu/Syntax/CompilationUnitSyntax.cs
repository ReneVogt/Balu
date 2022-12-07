using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a compilation unit, e.g. a Balu code file.
/// </summary>
public sealed class CompilationUnitSyntax : SyntaxNode
{
    /// <summary>
    /// The root expression of this compilation unit.
    /// </summary>
    public ExpressionSyntax Expression { get; }
    /// <summary>
    /// The eof token of this compilation unit.
    /// </summary>
    public SyntaxToken EndOfFileToken { get; }

    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children 
    {
        get
        {
            yield return Expression;
            yield return EndOfFileToken;
        }
    }

    internal CompilationUnitSyntax(ExpressionSyntax expression, SyntaxToken endOfFileToken) =>
        (Expression, EndOfFileToken) = (expression, endOfFileToken);

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var expression = (ExpressionSyntax)visitor.Visit(Expression);
        var eof = (SyntaxToken)visitor.Visit(EndOfFileToken);
        return expression != Expression || eof != EndOfFileToken ? CompilationUnit(expression, eof) : this;
    }
}
