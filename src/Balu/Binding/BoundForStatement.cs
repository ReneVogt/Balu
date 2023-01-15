using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundForStatement : BoundLoopStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

    public VariableSymbol Variable { get; }
    public BoundExpression LowerBound { get; }
    public BoundExpression UpperBound { get; }
    public BoundStatement Body { get; }

    public BoundForStatement(SyntaxNode syntax, VariableSymbol variable, BoundExpression lowerBound, BoundExpression upperBound, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : base(syntax, breakLabel, continueLabel)
    {
        Variable = variable;
        LowerBound = lowerBound; 
        UpperBound = upperBound;
        Body = body;
    }

    public override string ToString() => $"{Kind} \"{Variable.Name}\"";
}