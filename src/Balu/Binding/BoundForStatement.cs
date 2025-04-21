using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundForStatement(SyntaxNode syntax, VariableSymbol variable, BoundExpression lowerBound, BoundExpression upperBound, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : BoundLoopStatement(syntax, breakLabel, continueLabel)
{
    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

    public VariableSymbol Variable { get; } = variable;
    public BoundExpression LowerBound { get; } = lowerBound;
    public BoundExpression UpperBound { get; } = upperBound;
    public BoundStatement Body { get; } = body;

    public override string ToString() => $"{Kind} \"{Variable.Name}\"";
}