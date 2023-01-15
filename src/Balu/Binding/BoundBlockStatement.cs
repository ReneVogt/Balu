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

}