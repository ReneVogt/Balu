using Balu.Syntax;

namespace Balu.Binding;

abstract class BoundStatement : BoundNode
{
    private protected BoundStatement(SyntaxNode syntax) : base(syntax) { }
}
