using System.Collections.Immutable;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundBlockStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;

    public ImmutableArray<BoundStatement> Statements { get; }

    public BoundBlockStatement(SyntaxNode syntax, ImmutableArray<BoundStatement> statements) : base(syntax)
    {
        Statements = statements;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var transformed = RewriteList(rewriter, Statements);
        return transformed == Statements ? this : new (Syntax, transformed);
    }
}