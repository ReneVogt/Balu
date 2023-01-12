using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class FunctionDeclarationSyntax : MemberSyntax
{
    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
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
            if (TypeClause is not null) yield return TypeClause;
            yield return Body;
        }
    }
    public SyntaxToken FunctionKeyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
    public SyntaxToken ClosedParenthesis { get; }
    public TypeClauseSyntax? TypeClause { get; }
    public BlockStatementSyntax Body { get; }

    internal FunctionDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken functionKeyword, SyntaxToken identifier, SyntaxToken openParenthesis,
                                       SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closedParenthesis, TypeClauseSyntax? type,
                                       BlockStatementSyntax body)
        : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        FunctionKeyword = functionKeyword ?? throw new ArgumentNullException(nameof(functionKeyword));
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        OpenParenthesis = openParenthesis ?? throw new ArgumentNullException(nameof(openParenthesis));
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ClosedParenthesis = closedParenthesis ?? throw new ArgumentNullException(nameof(closedParenthesis));
        TypeClause = type;
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

}