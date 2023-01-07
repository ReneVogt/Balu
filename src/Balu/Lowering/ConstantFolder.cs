using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

    static BoundBlockStatement RemoveFixedConditionalGotos(BoundBlockStatement block)
    {
        ImmutableArray<BoundStatement>.Builder? resultBuilder = null;
        List<BoundLabelStatement> allLabels = new();
        HashSet<BoundLabel> usedLabels = new();

        for (int i = 0; i < block.Statements.Length; i++)
        {
            var statement = block.Statements[i];

            if (statement.Kind == BoundNodeKind.LabelStatement)
                allLabels.Add((BoundLabelStatement)statement);

            if (statement.Kind != BoundNodeKind.ConditionalGotoStatement ||
                statement is not BoundConditionalGotoStatement { Condition.Kind: BoundNodeKind.LiteralExpression } cgs)
            {
                if (statement.Kind == BoundNodeKind.ConditionalGotoStatement)
                    usedLabels.Add(((BoundConditionalGotoStatement)statement).Label);
                if (statement.Kind == BoundNodeKind.GotoStatement)
                    usedLabels.Add(((BoundGotoStatement)statement).Label);

                resultBuilder?.Add(statement);
                continue;
            }

            if (resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<BoundStatement>(block.Statements.Length);
                resultBuilder.AddRange(block.Statements.Take(i));
            }

            if ((bool)((BoundLiteralExpression)cgs.Condition).Value == cgs.JumpIfTrue)
            {
                usedLabels.Add(cgs.Label);
                resultBuilder.Add(new BoundGotoStatement(cgs.Label));
            }
        }

        if (allLabels.Count > usedLabels.Count)
        {
            if (resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<BoundStatement>(block.Statements.Length);
                resultBuilder.AddRange(block.Statements);
            }

            allLabels.RemoveAll(labelStatement => usedLabels.Contains(labelStatement.Label));
            allLabels.ForEach(labelStatement => resultBuilder.Remove(labelStatement));
        }

        return resultBuilder is null ? block : new (resultBuilder.ToImmutable());
    }

    public static BoundBlockStatement FoldConstants(BoundBlockStatement statement) => RemoveFixedConditionalGotos((BoundBlockStatement)new ConstantFolder().Visit(statement));
}