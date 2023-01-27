using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundEndScopeStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.EndScopeStatement;

    public BoundEndScopeStatement(SyntaxNode syntax)
        : base(syntax)
    {
    }
}