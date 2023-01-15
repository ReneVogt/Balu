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
    
    public BoundBinaryExpression(SyntaxNode syntax, BoundExpression left, BoundBinaryOperator op, BoundExpression right) : base(syntax)
    {
        Left = left;
        Operator = op;
        Right = right;
        Constant = ConstantFolder.ComputeConstant(left, op, right);
        HasSideEffects = Left.HasSideEffects || Right.HasSideEffects;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var left = (BoundExpression)rewriter.Visit(Left);
        var right = (BoundExpression)rewriter.Visit(Right);
        return left == Left && right == Right ? this : new (Syntax, left, Operator, right);
    }

    public override string ToString() => $"{Left} {Operator.SyntaxKind.GetText()} {Right}";
}