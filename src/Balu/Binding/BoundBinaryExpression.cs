using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundBinaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant { get; }
    public override bool HasSideEffects { get; }

    public BoundExpression Left { get; }
    public BoundBinaryOperator Operator { get; }
    public BoundExpression Right { get; }
    
    public BoundBinaryExpression(SyntaxNode syntax, BoundExpression left, BoundBinaryOperator @operator, BoundExpression right) : base(syntax)
    {
        Left = left;
        Operator = @operator;
        Right = right;
        Constant = ConstantFolder.ComputeConstant(left, @operator, right);
        HasSideEffects = Left.HasSideEffects || Right.HasSideEffects;
    }

    public override string ToString() => $"{Left} {Operator.SyntaxKind.GetText()} {Right}";
}
