using Balu.Binding;

namespace Balu.Lowering;

sealed class ConstantFolder : BoundTreeRewriter
{
    readonly VariableDictionary knownVariables = new();

    ConstantFolder(){}

    protected override BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        var reduced = (BoundVariableDeclarationStatement)base.VisitBoundVariableDeclarationStatement(variableDeclarationStatement);
        if (reduced.Variable.ReadOnly && reduced.Expression.Kind == BoundNodeKind.LiteralExpression)
            knownVariables[reduced.Variable] = ((BoundLiteralExpression)reduced.Expression).Value;

        return reduced;
    }
    protected override BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        var expression = (BoundExpression)base.VisitBoundUnaryExpression(unaryExpression);
        if (expression.Kind == BoundNodeKind.UnaryExpression)
        {
            var unary = (BoundUnaryExpression)expression;
            if (unary.Operand.Kind == BoundNodeKind.LiteralExpression)
                return new BoundLiteralExpression(unary.Operator.Apply(((BoundLiteralExpression)unary.Operand).Value));
        }

        return expression;
    }
    protected override BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        var expression = (BoundExpression)base.VisitBoundBinaryExpression(binaryExpression);
        if (expression.Kind == BoundNodeKind.BinaryExpression)
        {
            var binary = (BoundBinaryExpression)expression;
            if (binary.Left.Kind == BoundNodeKind.LiteralExpression && binary.Right.Kind == BoundNodeKind.LiteralExpression)
                return new BoundLiteralExpression(binary.Operator.Apply(((BoundLiteralExpression)binary.Left).Value, ((BoundLiteralExpression)binary.Right).Value));
        }

        return expression;
    }
    protected override BoundNode VisitBoundVariableExpression(BoundVariableExpression variableExpression) =>
        knownVariables.TryGetValue(variableExpression.Variable, out var value) ? new BoundLiteralExpression(value!) : variableExpression;

    protected override BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        var condition = (BoundExpression)Visit(conditionalGotoStatement.Condition);
        if (condition.Kind != BoundNodeKind.LiteralExpression ||
            (bool)((BoundLiteralExpression)condition).Value == conditionalGotoStatement.JumpIfTrue)
            return new BoundGotoStatement(conditionalGotoStatement.Label);

        return condition == conditionalGotoStatement.Condition
                   ? conditionalGotoStatement
                   : new BoundConditionalGotoStatement(conditionalGotoStatement.Label, condition, conditionalGotoStatement.JumpIfTrue);
    }

    public static BoundBlockStatement FoldConstants(BoundBlockStatement statement) => (BoundBlockStatement)new ConstantFolder().Visit(statement);
}