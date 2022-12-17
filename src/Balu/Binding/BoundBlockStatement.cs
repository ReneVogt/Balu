using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Binding;

sealed class BoundBlockStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    public override IEnumerable<BoundNode> Children => Statements;

    public ImmutableArray<BoundStatement> Statements { get; }

    public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
    {
        Statements = statements;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var transformed = VisitList(visitor, Statements);
        return transformed == Statements ? this : new (transformed);
    }
}