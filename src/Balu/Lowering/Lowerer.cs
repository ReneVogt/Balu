using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeVisitor
{
    int labelCount;
    BoundLabel GenerateNextLabel() => new($"Label{labelCount++}");

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
            var gotoStatement = new BoundConditionalGotoStatement(endLabel.Label, ifStatemnet.Condition, false);
            result = new BoundBlockStatement(ImmutableArray.Create(gotoStatement, ifStatemnet.ThenStatement, endLabel));
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
            var gotoElse = new BoundConditionalGotoStatement(elseLabel.Label, ifStatemnet.Condition, false);
            var gotoEnd = new BoundGotoStatement(endLabel.Label);
            var builder = ImmutableArray.CreateBuilder<BoundStatement>(6);
            builder.Add(gotoElse);
            builder.Add(ifStatemnet.ThenStatement);
            builder.Add(gotoEnd);
            builder.Add(elseLabel);
            builder.Add(ifStatemnet.ElseStatement);
            builder.Add(endLabel);
            result = new BoundBlockStatement(builder.ToImmutable());
        }

        return Visit(result);
    }
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
        var gotoEnd = new BoundConditionalGotoStatement(endLabel.Label, whileStatement.Condition, false);
        var gotoCheck = new BoundGotoStatement(checkLabel.Label);
        var builder = ImmutableArray.CreateBuilder<BoundStatement>(5);
        builder.Add(checkLabel);
        builder.Add(gotoEnd);
        builder.Add(whileStatement.Body);
        builder.Add(gotoCheck);
        builder.Add(endLabel);
        var result = new BoundBlockStatement(builder.ToImmutable());
        return Visit(result);
    }
    protected override BoundNode VisitBoundDoWhileStatement(BoundDoWhileStatement doWhileStatement)
    {
        /*
         * do                      start:
         *   <body>                <body>
         * while <condition>       <condition>
         *                         GotoTrue <start>
         */

        var startLabel = new BoundLabelStatement(GenerateNextLabel());
        var gotoStart = new BoundConditionalGotoStatement(startLabel.Label, doWhileStatement.Condition);
        var result = new BoundBlockStatement(ImmutableArray.Create(startLabel, doWhileStatement.Body, gotoStart));
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
         *     var <tmp> = <upper>
         *     while <var> <= <tmp>
         *     {
         *       <body>
         *       <var> = <var> + 1
         *     }
         *   }
         */

        var loopVariable = new BoundVariableExpression(forStatement.Variable);
        var loopVariableDeclaration = new BoundVariableDeclarationStatement(forStatement.Variable, forStatement.LowerBound);

        var upperVariableSymbol = new VariableSymbol("upperBound", false, TypeSymbol.Integer);
        var upperVariableDeclaration = new BoundVariableDeclarationStatement(upperVariableSymbol, forStatement.UpperBound);

        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(
                forStatement.Variable,
                new BoundBinaryExpression(
                    loopVariable,
                    BoundBinaryOperator.BinaryPlus,
                    new BoundLiteralExpression(1))));

        var whileCondition = new BoundBinaryExpression(
            loopVariable,
            BoundBinaryOperator.LessOrEquals,
            new BoundVariableExpression(upperVariableSymbol));
        var whileBody = new BoundBlockStatement(ImmutableArray.Create(forStatement.Body, increment));
        var whileStatement = new BoundWhileStatement(whileCondition, whileBody);

        var rewritten = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(loopVariableDeclaration, upperVariableDeclaration, whileStatement));
        return Visit(rewritten);
    }

    static BoundBlockStatement Flatten(BoundStatement statement)
    {
        var resultBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
        Stack<BoundStatement> stack = new();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current is BoundBlockStatement { Statements: var statements })
                foreach (var s in statements.Reverse())
                    stack.Push(s);
            else
                resultBuilder.Add(current);
        }

        return new (resultBuilder.ToImmutable());
    }
    public static BoundBlockStatement Lower(BoundStatement statement) => Flatten((BoundStatement)new Lowerer().Visit(statement));
}
