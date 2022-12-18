namespace Balu.Binding;

abstract class BoundLoopStatement : BoundStatement
{
    public BoundLabel BreakLabel { get; }
    public BoundLabel ContinueLabel { get; }

    private protected BoundLoopStatement(BoundLabel breakLabel, BoundLabel continueLabel) =>
        (BreakLabel, ContinueLabel) = (breakLabel, continueLabel);
}
