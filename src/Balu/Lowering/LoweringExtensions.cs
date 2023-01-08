using Balu.Binding;
using Balu.Symbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Lowering;

static class LoweringExtensions
{
    public static BoundBlockStatement Lower(this BoundStatement statement, FunctionSymbol? containingFunction) => Lowerer.Lower(statement, containingFunction);
    public static BoundBlockStatement FoldConstants(this BoundBlockStatement statement) => ConstantFolder.FoldConstants(statement);
    public static BoundBlockStatement OptimizeConstantConditionalGotos(this BoundBlockStatement block)
    {
        ImmutableArray<BoundStatement>.Builder? resultBuilder = null;
        var allLabelStatements = block.Statements.Where(statement => statement.Kind == BoundNodeKind.LabelStatement)
                             .Cast<BoundLabelStatement>().ToImmutableArray();
        HashSet<BoundLabel> usedLabels = new();

        for (int i = 0; i < block.Statements.Length; i++)
        {
            var statement = block.Statements[i];

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

        if (allLabelStatements.Length > usedLabels.Count)
        {
            if (resultBuilder is null)
            {
                resultBuilder = ImmutableArray.CreateBuilder<BoundStatement>(block.Statements.Length);
                resultBuilder.AddRange(block.Statements);
            }

            foreach(var unusedLabel in allLabelStatements.Where(labelStatement => !usedLabels.Contains(labelStatement.Label)))
                resultBuilder.Remove(unusedLabel);
        }

        return resultBuilder is null ? block : new(resultBuilder.ToImmutable());
    }
}