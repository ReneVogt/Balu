using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Binding;

sealed class BoundBlockStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    public override IEnumerable<BoundNode> Children => Statements;

    public ImmutableArray<BoundStatement> Statements { get; }

    public BoundBlockStatement(params BoundStatement[] statements) : this(statements.AsEnumerable()){}
    public BoundBlockStatement(IEnumerable<BoundStatement> statements)
    {
        Statements = statements.ToImmutableArray();
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        List<BoundStatement>? transformedChildren = null;
        for (int i = 0; i < Statements.Length; i++)
        {
            var transformed = (BoundStatement)visitor.Visit(Statements[i]);
            if (transformed != Statements[i])
            {
                if (transformedChildren is null)
                {
                    transformedChildren = new(Statements.Length);
                    transformedChildren.AddRange(Statements.Take(i));
                }
            }

            transformedChildren?.Add(transformed);
        }

        return transformedChildren is null ? this : new (transformedChildren);
    }
}