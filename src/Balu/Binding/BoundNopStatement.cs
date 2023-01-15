using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundNopStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.NopStatement;

    internal BoundNopStatement(SyntaxNode syntax) : base(syntax){}

    public override string ToString() => "nop";
}
