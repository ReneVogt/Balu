using System.Collections.Immutable;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundBlockStatement(SyntaxNode syntax, ImmutableArray<BoundStatement> statements) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;

    public ImmutableArray<BoundStatement> Statements { get; } = statements;
}