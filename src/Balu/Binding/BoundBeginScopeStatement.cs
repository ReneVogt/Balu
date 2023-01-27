using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundBeginScopeStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.BeginScopeStatement;

    public BoundBeginScopeStatement(SyntaxNode syntax)
        : base(syntax)
    {
    }
}