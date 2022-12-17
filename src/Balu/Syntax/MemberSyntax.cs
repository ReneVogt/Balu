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
    /// <param name="functionKeyword">The <see cref="SyntaxToken"/> of the 'function' keyword.</param>
    /// <param name="identifier">The <see cref="SyntaxToken"/> of the function's name.</param>
    /// <param name="openParenthesis">The <see cref="SyntaxToken"/> of the opening parenthesis.</param>
    /// <param name="parameters">The function's parameters..</param>
    /// <param name="closedParenthesis">The <see cref="SyntaxToken"/> of the closing parenthesis.</param>
    /// <param name="type">The optional <see cref="TypeClauseSyntax"/> declaring hte function's return type.</param>
    /// <param name="body">The function's body.</param>
    /// <returns>A new <see cref="FunctionDeclarationSyntax"/> instance.</returns>
    /// <exception cref="ArgumentNullException">An argument is <c>null</c>.</exception>
    public static FunctionDeclarationSyntax FunctionDeclaration(SyntaxToken functionKeyword, SyntaxToken identifier, SyntaxToken openParenthesis,
                                                                SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closedParenthesis, TypeClauseSyntax? type, BlockStatementSyntax body) =>
        new(functionKeyword ?? throw new ArgumentNullException(nameof(functionKeyword)),
            identifier ?? throw new ArgumentNullException(nameof(identifier)),
            openParenthesis ?? throw new ArgumentNullException(nameof(openParenthesis)),
            parameters ?? throw new ArgumentNullException(nameof(parameters)),
            closedParenthesis ?? throw new ArgumentNullException(nameof(closedParenthesis)),
            type,
            body ?? throw new ArgumentNullException(nameof(body)));

}