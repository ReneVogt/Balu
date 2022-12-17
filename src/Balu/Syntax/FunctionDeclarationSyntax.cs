using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a function declaration in the Balu language.
/// </summary>
public sealed class FunctionDeclarationSyntax : MemberSyntax
{
    /// <inheritdoc />
    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
    /// <inheritdoc />
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return FunctionKeyword;
            yield return Identifier;
            yield return OpenParenthesis;
            foreach(var node in Parameters.ElementsWithSeparators)
                yield return node;
            yield return ClosedParenthesis;
            yield return TypeClause;
        }
    }

    /// <summary>
    /// The 'function' keyword.
    /// </summary>
    public SyntaxToken FunctionKeyword { get; }
    /// <summary>
    /// The <see cref="SyntaxToken"/> for the function's name.
    /// </summary>
    public SyntaxToken Identifier { get; }
    /// <summary>
    /// The opening parenthesis token.
    /// </summary>
    public SyntaxToken OpenParenthesis { get; }
    /// <summary>
    /// The function's parameters.
    /// </summary>
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    /// <summary>
    /// The closing parenthesis token.
    /// </summary>
    public SyntaxToken ClosedParenthesis { get; }
    /// <summary>
    /// The function's type clause.
    /// </summary>
    public TypeClauseSyntax TypeClause { get; }

    internal FunctionDeclarationSyntax(SyntaxToken functionKeyword, SyntaxToken identifier, SyntaxToken openParenthesis,
                                       SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closedParenthesis, TypeClauseSyntax type)
    {
        FunctionKeyword = functionKeyword;
        Identifier = identifier;
        OpenParenthesis = openParenthesis;
        Parameters = parameters;
        ClosedParenthesis = closedParenthesis;
        TypeClause = type;
    }

    /// <inheritdoc />
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var functionKeyword = (SyntaxToken)visitor.Visit(FunctionKeyword);
        var identifier = (SyntaxToken)visitor.Visit(Identifier);
        var openParenthesis = (SyntaxToken)visitor.Visit(OpenParenthesis);
        var parameters = VisitList(visitor, Parameters);
        var closedParenthesis = (SyntaxToken)visitor.Visit(ClosedParenthesis);
        var type = (TypeClauseSyntax)visitor.Visit(TypeClause);

        return functionKeyword == FunctionKeyword && identifier == Identifier && openParenthesis == OpenParenthesis && parameters == Parameters &&
               closedParenthesis == ClosedParenthesis && TypeClause == type
                   ? this
                   : FunctionDeclaration(functionKeyword, identifier, openParenthesis, parameters, closedParenthesis, type);
    }
}