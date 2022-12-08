using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Binding;

sealed class BoundBlockStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    public ImmutableArray<BoundStatement> Statements { get; }

    public BoundBlockStatement(IEnumerable<BoundStatement> statements)
    {
        Statements = statements.ToImmutableArray();
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var transformed = Statements.Select(visitor.Visit).Cast<BoundStatement>().ToList();
        return Statements.SequenceEqual(transformed) ? this : new (transformed);
    }
}