using Balu.Syntax;
using System.Collections.Generic;
using Balu.Text;

namespace Balu.Binding;

sealed class BoundSequencePointStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.SequencePointStatement;
    public override IEnumerable<BoundNode> Children
    {
        get { yield return Statement; }
    }

    public BoundStatement Statement { get; }
    public TextLocation Location { get; }

    public BoundSequencePointStatement(SyntaxNode syntax, BoundStatement statement, TextLocation location)
        : base(syntax)
    {
        Statement = statement;
        Location = location;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var statement = (BoundStatement)rewriter.Visit(this);
        return statement == Statement ? this : new(Syntax, statement, Location);
    }
}