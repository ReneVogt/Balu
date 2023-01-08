using Balu.Symbols;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class BoundBinaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant { get; }
    public override IEnumerable<BoundNode> Children
    {
        get
        {
            yield return Left;
            yield return Right;
        }
    }

    public BoundExpression Left { get; }
    public BoundBinaryOperator Operator { get; }
    public BoundExpression Right { get; }
    
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
        Constant = ConstantFolder.ComputeConstant(left, op, right);
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var left = (BoundExpression)rewriter.Visit(Left);
        var right = (BoundExpression)rewriter.Visit(Right);
        return left == Left && right == Right ? this : new (left, Operator, right);
    }

    public override string ToString() => $"{Left} {Operator.SyntaxKind.GetText()} {Right}";
}