using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundNopStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.NopStatement;

    internal BoundNopStatement(SyntaxNode syntax) : base(syntax){}

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => "nop";
}
