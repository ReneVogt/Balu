using System;
using System.Collections.Generic;

namespace Balu.Syntax;

public sealed class GlobalStatementSyntax : MemberSyntax
{
    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public override IEnumerable<SyntaxNode> Children
    {
        get { yield return Statement; }
    }

    public StatementSyntax Statement { get; }

    public GlobalStatementSyntax(SyntaxTree? syntaxTree, StatementSyntax statement) : base(syntaxTree)
    {
        Statement = statement ?? throw new ArgumentNullException(nameof(statement)) ;
    }

    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var statement = (StatementSyntax)visitor.Visit(Statement);
        return statement == Statement ? this : new(null, statement);
    }
}
