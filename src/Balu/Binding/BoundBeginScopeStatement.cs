using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundBeginScopeStatement(SyntaxNode syntax) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.BeginScopeStatement;
}