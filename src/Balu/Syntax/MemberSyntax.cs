using System;

namespace Balu.Syntax;

/// <summary>
/// Abstract base class for member syntax.
/// </summary>
public abstract class MemberSyntax : SyntaxNode
{
    /// <summary>
    /// Creates a new <see cref="GlobalStatementSyntax"/> from the given <see cref="StatementSyntax"/>.
    /// </summary>
    /// <param name="statement">The root <see cref="StatementSyntax"/> of the compilation unit.</param>
    /// <returns>A new <see cref="GlobalStatementSyntax"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="statement"/>  is <c>null</c>.</exception>
    public static GlobalStatementSyntax GlobalStatement(StatementSyntax statement) =>
        new(statement ?? throw new ArgumentNullException(nameof(statement)));
    /// <summary>
    /// Creates a new <see cref="FunctionDeclarationSyntax"/> from the given elements/>.
    /// </summary>
    /// <returns>A new <see cref="FunctionDeclarationSyntax"/> instance.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static FunctionDeclarationSyntax FunctionDeclaration(SyntaxToken functionKeyword, SyntaxToken identifier, SyntaxToken openParenthesis,
                                                                SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closedParenthesis, TypeClauseSyntax type) =>
        new(functionKeyword ?? throw new ArgumentNullException(nameof(functionKeyword)),
            identifier ?? throw new ArgumentNullException(nameof(identifier)),
            openParenthesis ?? throw new ArgumentNullException(nameof(openParenthesis)),
            parameters ?? throw new ArgumentNullException(nameof(parameters)),
            closedParenthesis ?? throw new ArgumentNullException(nameof(closedParenthesis)),
            type ?? throw new ArgumentNullException(nameof(type)));

}