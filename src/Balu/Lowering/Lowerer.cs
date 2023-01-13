using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;

using static Balu.Binding.BoundNodeFactory;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeRewriter
{
    int labelCount;

    BoundLabel GenerateNextLabel() => new($"Label{labelCount++}");

    protected override BoundNode VisitBoundIfStatement(BoundIfStatement ifStatement)
    {
        BoundStatement result;

        var syntax = ifStatement.Syntax;

        if (ifStatement.ElseStatement is null)
        {
            /*
             *   if <condition>         GotoIfFalse <condition> <end>
             *      <then>              <then>
             *                          end:
             */
            var endLabel = GenerateNextLabel();
            result = Block(syntax,
                           GotoFalse(syntax, endLabel, ifStatement.Condition),
                           ifStatement.ThenStatement,
                           Label(syntax, endLabel));

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

            var elseLabel = GenerateNextLabel();
            var endLabel = GenerateNextLabel();
            result = Block(syntax,
                           GotoFalse(syntax, elseLabel, ifStatement.Condition),
                           ifStatement.ThenStatement,
                           Goto(syntax, endLabel),
                           Label(syntax, elseLabel),
                           ifStatement.ElseStatement,
                           Label(syntax, endLabel));
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

        var syntax = whileStatement.Syntax;

        var result = Block(syntax,
                           Label(syntax, whileStatement.ContinueLabel),
                           GotoFalse(syntax, whileStatement.BreakLabel, whileStatement.Condition),
                           whileStatement.Body,
                           Goto(syntax, whileStatement.ContinueLabel),
                           Label(syntax, whileStatement.BreakLabel));
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

        var syntax = doWhileStatement.Syntax;
        var startLabel = GenerateNextLabel();
        var result = Block(syntax,
                           Label(syntax, startLabel),
                           doWhileStatement.Body,
                           Label(syntax, doWhileStatement.ContinueLabel),
                           GotoTrue(syntax, startLabel, doWhileStatement.Condition),
                           Label(syntax, doWhileStatement.BreakLabel));
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

        var syntax = forStatement.Syntax;


        var upperVariableDeclaration = ConstantDeclaration(syntax, "<upperBound>", forStatement.UpperBound);
        var result = Block(syntax,
                           VariableDeclaration(syntax, forStatement.Variable, forStatement.LowerBound),
                           upperVariableDeclaration,
                           While(syntax,
                                 LessOrEqual(syntax, 
                                             Variable(syntax, forStatement.Variable), 
                                             Variable(syntax, upperVariableDeclaration.Variable)),
                                 Block(syntax,
                                       forStatement.Body,
                                       Label(syntax, forStatement.ContinueLabel),
                                       Increment(syntax, Variable(syntax, forStatement.Variable))),
                                 forStatement.BreakLabel,
                                 new($"<lowcontinue{labelCount++}>"))
        );
        return Visit(result);
    }
    protected override BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        if (conditionalGotoStatement.Condition.Constant is null) return conditionalGotoStatement;
        return (bool)conditionalGotoStatement.Condition.Constant.Value == conditionalGotoStatement.JumpIfTrue
               ? Visit(new BoundGotoStatement(conditionalGotoStatement.Syntax, conditionalGotoStatement.Label))
               : Visit(new BoundNopStatement(conditionalGotoStatement.Syntax));
    }

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
    public static BoundBlockStatement Lower(BoundStatement statement, FunctionSymbol? containingFunction) => RemoveDeadCode(Flatten((BoundStatement)new Lowerer().Visit(statement), containingFunction));
}
