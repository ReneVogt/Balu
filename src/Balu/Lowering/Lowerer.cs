using System.Collections.Generic;
using System.Linq;
using Balu.Binding;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeVisitor
{
    int labelCount;
    LabelSymbol GenerateNextLabel() => new($"Label{labelCount++}");

    protected override BoundNode VisitBoundIfStatement(BoundIfStatement ifStatemnet)
    {
        BoundStatement result;

        if (ifStatemnet.ElseStatement is null)
        {
            /*
             *   if <condition>         GotoIfFalse <condition> <end>
             *      <then>              <then>
             *                          end:
             */
            var endLabel = new BoundLabelStatement(GenerateNextLabel());
            var gotoStatement = new BoundConditionalGotoStatement(endLabel.Label, ifStatemnet.Condition, true);
            result = new BoundBlockStatement(
                gotoStatement,
                ifStatemnet.ThenStatement,
                endLabel);
        }
        else
        {
            /*
             *   if <condition>         GotoIfFalse <condition> <elseLabel>
             *      <then>              <then>
             *   else                   Goto <end>
             *      <else>              elseLabel:
             *                          <else>
             *                          end:
             */

            var elseLabel = new BoundLabelStatement(GenerateNextLabel());
            var endLabel = new BoundLabelStatement(GenerateNextLabel());
            var gotoElse = new BoundConditionalGotoStatement(elseLabel.Label, ifStatemnet.Condition, true);
            var gotoEnd = new BoundGotoStatement(endLabel.Label);
            result = new BoundBlockStatement(
                gotoElse,
                ifStatemnet.ThenStatement,
                gotoEnd,
                elseLabel,
                ifStatemnet.ElseStatement,
                endLabel);
        }

        return Visit(result);
    }
    /// <inheritdoc />
    protected override BoundNode VisitBoundWhileStatement(BoundWhileStatement whileStatement)
    {
        /*
         * while <condition>       check:
         *   <body>                GotoIfFalse <condition> <end>
         *                         <body>
         *                         Goto <check>
         *                         end:
         */

        var checkLabel = new BoundLabelStatement(GenerateNextLabel());
        var endLabel = new BoundLabelStatement(GenerateNextLabel());
        var gotoEnd = new BoundConditionalGotoStatement(endLabel.Label, whileStatement.Condition, true);
        var gotoCheck = new BoundGotoStatement(checkLabel.Label);
        var result = new BoundBlockStatement(
            checkLabel,
            gotoEnd,
            whileStatement.Body,
            gotoCheck,
            endLabel
        );
        return Visit(result);
    }
    protected override BoundNode VisitBoundForStatement(BoundForStatement forStatement)
    {
        /*
         * Source statement:
         *   for <var> = <lower> to <upper>
         *     <body>
         *
         * Target statement:
         *   {
         *     var <var> = <lower>
         *     while <var> <= <upper>
         *     {
         *       <body>
         *       <var> = <var> + 1
         *     }
         *   }
         */

        var variable = new BoundVariableExpression(forStatement.Variable);
        var declaration = new BoundVariableDeclarationStatement(forStatement.Variable, forStatement.LowerBound);

        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(
                forStatement.Variable,
                new BoundBinaryExpression(
                    variable,
                    BoundBinaryOperator.BinaryPlus,
                    new BoundLiteralExpression(1))));

        var whileCondition = new BoundBinaryExpression(
            variable,
            BoundBinaryOperator.LessOrEquals,
            forStatement.UpperBound);
        var whileBody = new BoundBlockStatement(forStatement.Body, increment);
        var whileStatement = new BoundWhileStatement(whileCondition, whileBody);

        var rewritten = new BoundBlockStatement(declaration, whileStatement);
        return Visit(rewritten);
    }

    static BoundBlockStatement Flatten(BoundStatement statement)
    {
        List<BoundStatement> result = new();
        Stack<BoundStatement> stack = new();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current is BoundBlockStatement { Statements: var statements })
                foreach (var s in statements.Reverse())
                    stack.Push(s);
            else
                result.Add(current);
        }

        return new (result);
    }
    public static BoundBlockStatement Lower(BoundStatement statement) => Flatten((BoundStatement)new Lowerer().Visit(statement));
}
