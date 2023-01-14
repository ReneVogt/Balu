using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundUnaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operator.Type;
    public override BoundConstant? Constant { get; }
    public override bool HasSideEffects { get; }

    public BoundUnaryOperator Operator { get; }
    public BoundExpression Operand { get; }

    public BoundUnaryExpression(SyntaxNode syntax, BoundUnaryOperator op, BoundExpression operand) : base(syntax)
    {
        Operator = op;
        Operand = operand;
        Constant = ConstantFolder.ComputeConstant(op, operand);
        HasSideEffects = operand.HasSideEffects;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var operand = (BoundExpression)rewriter.Visit(Operand);
        return operand == Operand ? this : new (Syntax, Operator, operand);
    }

    public override string ToString() => $"{Operator.SyntaxKind.GetText()} {Operand}";
}