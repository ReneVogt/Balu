using Balu.Syntax;

namespace Balu.Binding;

abstract class BoundLoopStatement : BoundStatement
{
    public BoundLabel BreakLabel { get; }
    public BoundLabel ContinueLabel { get; }

    private protected BoundLoopStatement(SyntaxNode syntax, BoundLabel breakLabel, BoundLabel continueLabel) : base(syntax)
    {
        BreakLabel = breakLabel;
        ContinueLabel = continueLabel;
    }
}
