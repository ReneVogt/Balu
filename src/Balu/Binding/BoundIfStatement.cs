using Balu.Syntax;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundIfStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Condition;
            yield return ThenStatement;
            if (ElseStatement is not null) yield return ElseStatement;
        }
    }

    public BoundExpression Condition { get; }
    public BoundStatement ThenStatement { get; }
    public BoundStatement? ElseStatement { get; }

    public BoundIfStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement thenStatement, BoundStatement? elseStatement)
        : base(syntax)
    {
        Condition = condition;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var condition = (BoundExpression)rewriter.Visit(Condition);
        var thenStatement = (BoundStatement)rewriter.Visit(ThenStatement);
        var elseStatement = ElseStatement is null ? null : (BoundStatement)rewriter.Visit(ElseStatement);
        return condition == Condition && thenStatement == ThenStatement && elseStatement == ElseStatement
                   ? this
                   : new (Syntax, condition, thenStatement, elseStatement);
    }
}