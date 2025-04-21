using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundEndScopeStatement(SyntaxNode syntax) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.EndScopeStatement;
}