using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeRewriter
{
    readonly FunctionSymbol? containingFunction;
    int labelCount;

    Lowerer(FunctionSymbol? containingFunction) => this.containingFunction = containingFunction;

    BoundLabel GenerateNextLabel() => new($"Label{labelCount++}");

    protected override BoundNode VisitBoundIfStatement(BoundIfStatement ifStatement)
    {
        BoundStatement result;

        if (ifStatement.ElseStatement is null)
        {
            /*
             *   if <condition>         GotoIfFalse <condition> <end>
             *      <then>              <then>
             *                          end:
             */
            var endLabel = new BoundLabelStatement(GenerateNextLabel());
            var gotoStatement = new BoundConditionalGotoStatement(endLabel.Label, ifStatement.Condition, false);
            result = new BoundBlockStatement(ImmutableArray.Create(gotoStatement, ifStatement.ThenStatement, endLabel));
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
            var gotoElse = new BoundConditionalGotoStatement(elseLabel.Label, ifStatement.Condition, false);
            var gotoEnd = new BoundGotoStatement(endLabel.Label);
            var builder = ImmutableArray.CreateBuilder<BoundStatement>(6);
            builder.Add(gotoElse);
            builder.Add(ifStatement.ThenStatement);
            builder.Add(gotoEnd);
            builder.Add(elseLabel);
            builder.Add(ifStatement.ElseStatement);
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

        var checkLabel = new BoundLabelStatement(whileStatement.ContinueLabel);
        var endLabel = new BoundLabelStatement(whileStatement.BreakLabel);
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
         *                         continue:
         * while <condition>       <condition>
         *                         GotoTrue <start>
         *                         break:
         */

        var startLabel = new BoundLabelStatement(GenerateNextLabel());
        var continueLabel = new BoundLabelStatement(doWhileStatement.ContinueLabel);
        var gotoStart = new BoundConditionalGotoStatement(startLabel.Label, doWhileStatement.Condition);
        var breakLabel = new BoundLabelStatement(doWhileStatement.BreakLabel);
        var builder = ImmutableArray.CreateBuilder<BoundStatement>(5);
        builder.Add(startLabel);
        builder.Add(doWhileStatement.Body);
        builder.Add(continueLabel);
        builder.Add(gotoStart);
        builder.Add(breakLabel);
        var result = new BoundBlockStatement(builder.ToImmutable());
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
         *       continue:
         *       <var> = <var> + 1
         *     }
         *   }
         */

        var loopVariable = new BoundVariableExpression(forStatement.Variable);
        var loopVariableDeclaration = new BoundVariableDeclarationStatement(forStatement.Variable, forStatement.LowerBound);

        var upperVariableSymbol = CreateVariable("upperBound", false, TypeSymbol.Integer);
        var upperVariableDeclaration = new BoundVariableDeclarationStatement(upperVariableSymbol, forStatement.UpperBound);

        var continueLabel = new BoundLabelStatement(forStatement.ContinueLabel);
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
        var whileBody = new BoundBlockStatement(ImmutableArray.Create(forStatement.Body, continueLabel, increment));
        var whileStatement = new BoundWhileStatement(whileCondition, whileBody, forStatement.BreakLabel, new ($"lowcontinue{labelCount++}"));

        var rewritten = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(loopVariableDeclaration, upperVariableDeclaration, whileStatement));
        return Visit(rewritten);
    }

    VariableSymbol CreateVariable(string name, bool readOnly, TypeSymbol type) =>
        containingFunction is null ? new GlobalVariableSymbol(name, readOnly, type) : new LocalVariableSymbol(name, readOnly, type);

    static BoundBlockStatement Flatten(BoundStatement statement, FunctionSymbol? containingFunction)
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

        if (containingFunction?.ReturnType == TypeSymbol.Void && (resultBuilder.Count == 0 || CanFallThrough(resultBuilder[^1])))
                resultBuilder.Add(new BoundReturnStatement(null));
        
        return new (resultBuilder.ToImmutable());

        static bool CanFallThrough(BoundStatement lastStatement)
        {
            if (lastStatement.Kind == BoundNodeKind.ReturnStatement ||
                lastStatement.Kind == BoundNodeKind.GotoStatement) return false;
            if (lastStatement.Kind != BoundNodeKind.ConditionalGotoStatement) return true;

            var cgs = (BoundConditionalGotoStatement)lastStatement;
            return cgs.Condition.Kind != BoundNodeKind.LiteralExpression || (bool)((BoundLiteralExpression)cgs.Condition).Value != cgs.JumpIfTrue;
        }
    }
    public static BoundBlockStatement Lower(BoundStatement statement, FunctionSymbol? containingFunction) => Flatten((BoundStatement)new Lowerer(containingFunction).Visit(statement), containingFunction);
}
