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
            var endLabel = new BoundLabelStatement(ifStatement.Syntax, GenerateNextLabel());
            var gotoStatement = new BoundConditionalGotoStatement(ifStatement.Syntax, endLabel.Label, ifStatement.Condition, false);
            result = new BoundBlockStatement(ifStatement.Syntax, ImmutableArray.Create(gotoStatement, ifStatement.ThenStatement, endLabel));
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

            var elseLabel = new BoundLabelStatement(ifStatement.Syntax, GenerateNextLabel());
            var endLabel = new BoundLabelStatement(ifStatement.Syntax, GenerateNextLabel());
            var gotoElse = new BoundConditionalGotoStatement(ifStatement.Syntax, elseLabel.Label, ifStatement.Condition, false);
            var gotoEnd = new BoundGotoStatement(ifStatement.Syntax, endLabel.Label);
            var builder = ImmutableArray.CreateBuilder<BoundStatement>(6);
            builder.Add(gotoElse);
            builder.Add(ifStatement.ThenStatement);
            builder.Add(gotoEnd);
            builder.Add(elseLabel);
            builder.Add(ifStatement.ElseStatement);
            builder.Add(endLabel);
            result = new BoundBlockStatement(ifStatement.Syntax, builder.ToImmutable());
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

        var checkLabel = new BoundLabelStatement(whileStatement.Syntax, whileStatement.ContinueLabel);
        var endLabel = new BoundLabelStatement(whileStatement.Syntax, whileStatement.BreakLabel);
        var gotoEnd = new BoundConditionalGotoStatement(whileStatement.Syntax, endLabel.Label, whileStatement.Condition, false);
        var gotoCheck = new BoundGotoStatement(whileStatement.Syntax, checkLabel.Label);
        var builder = ImmutableArray.CreateBuilder<BoundStatement>(5);
        builder.Add(checkLabel);
        builder.Add(gotoEnd);
        builder.Add(whileStatement.Body);
        builder.Add(gotoCheck);
        builder.Add(endLabel);
        var result = new BoundBlockStatement(whileStatement.Syntax, builder.ToImmutable());
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

        var startLabel = new BoundLabelStatement(doWhileStatement.Syntax, GenerateNextLabel());
        var continueLabel = new BoundLabelStatement(doWhileStatement.Syntax, doWhileStatement.ContinueLabel);
        var gotoStart = new BoundConditionalGotoStatement(doWhileStatement.Syntax, startLabel.Label, doWhileStatement.Condition);
        var breakLabel = new BoundLabelStatement(doWhileStatement.Syntax, doWhileStatement.BreakLabel);
        var builder = ImmutableArray.CreateBuilder<BoundStatement>(5);
        builder.Add(startLabel);
        builder.Add(doWhileStatement.Body);
        builder.Add(continueLabel);
        builder.Add(gotoStart);
        builder.Add(breakLabel);
        var result = new BoundBlockStatement(doWhileStatement.Syntax, builder.ToImmutable());
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

        var loopVariable = new BoundVariableExpression(forStatement.Syntax, forStatement.Variable);
        var loopVariableDeclaration = new BoundVariableDeclarationStatement(forStatement.Syntax, forStatement.Variable, forStatement.LowerBound);

        var upperVariableSymbol = CreateVariable("upperBound", true, TypeSymbol.Integer, forStatement.UpperBound.Constant);
        var upperVariableDeclaration = new BoundVariableDeclarationStatement(forStatement.Syntax, upperVariableSymbol, forStatement.UpperBound);

        var continueLabel = new BoundLabelStatement(forStatement.Syntax, forStatement.ContinueLabel);
        var increment = new BoundExpressionStatement(forStatement.Syntax,
                                                     new BoundAssignmentExpression(forStatement.Syntax,
                                                                                   forStatement.Variable,
                                                                                   new BoundBinaryExpression(forStatement.Syntax,
                                                                                       loopVariable,
                                                                                       BoundBinaryOperator.BinaryPlus,
                                                                                       new BoundLiteralExpression(forStatement.Syntax, 1))));

        var whileCondition = new BoundBinaryExpression(forStatement.Syntax,
                                                       loopVariable,
                                                       BoundBinaryOperator.LessOrEquals,
                                                       new BoundVariableExpression(forStatement.Syntax, upperVariableSymbol));
        var whileBody = new BoundBlockStatement(forStatement.Syntax, ImmutableArray.Create(forStatement.Body, continueLabel, increment));
        var whileStatement = new BoundWhileStatement(forStatement.Syntax, whileCondition, whileBody, forStatement.BreakLabel, new ($"lowcontinue{labelCount++}"));

        var rewritten = new BoundBlockStatement(forStatement.Syntax, ImmutableArray.Create<BoundStatement>(loopVariableDeclaration, upperVariableDeclaration, whileStatement));
        return Visit(rewritten);
    }
    protected override BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        if (conditionalGotoStatement.Condition.Constant is null) return conditionalGotoStatement;
        return (bool)conditionalGotoStatement.Condition.Constant.Value == conditionalGotoStatement.JumpIfTrue
               ? Visit(new BoundGotoStatement(conditionalGotoStatement.Syntax, conditionalGotoStatement.Label))
               : Visit(new BoundNopStatement(conditionalGotoStatement.Syntax));
    }

    VariableSymbol CreateVariable(string name, bool readOnly, TypeSymbol type, BoundConstant? constant) =>
        containingFunction is null ? new GlobalVariableSymbol(name, readOnly, type, constant) : new LocalVariableSymbol(name, readOnly, type, constant);

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
                resultBuilder.Add(new BoundReturnStatement(statement.Syntax, null));
        
        return new (statement.Syntax, resultBuilder.ToImmutable());

        static bool CanFallThrough(BoundStatement lastStatement) => 
            lastStatement.Kind != BoundNodeKind.ReturnStatement &&
            lastStatement.Kind != BoundNodeKind.GotoStatement;
    }
    static BoundBlockStatement RemoveDeadCode(BoundBlockStatement statement)
    {
        var flow = ControlFlowGraph.Create(statement);
        var reachableStatements = flow.Blocks.SelectMany(block => block.Statements).ToHashSet();
        if (reachableStatements.Count == statement.Statements.Length) return statement;
        var builder = statement.Statements.ToBuilder();
        for (int i = builder.Count - 1; i >= 0; i--)
        {
            if (!reachableStatements.Contains(builder[i]))
                builder.RemoveAt(i);
        }
        return new (statement.Syntax, builder.ToImmutable());
    }
    public static BoundBlockStatement Lower(BoundStatement statement, FunctionSymbol? containingFunction) => RemoveDeadCode(Flatten((BoundStatement)new Lowerer(containingFunction).Visit(statement), containingFunction));
}
