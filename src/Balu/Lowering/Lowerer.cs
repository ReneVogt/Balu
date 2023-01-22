﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Binding;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using static Balu.Binding.BoundNodeFactory;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeRewriter
{
    int labelCount;

    BoundLabel GenerateNextLabel() => new($"Label{labelCount++}");

    protected override BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        if (conditionalGotoStatement.Condition.Constant is null) return conditionalGotoStatement;
        return (bool)conditionalGotoStatement.Condition.Constant.Value == conditionalGotoStatement.JumpIfTrue
                   ? Visit(new BoundGotoStatement(conditionalGotoStatement.Syntax, conditionalGotoStatement.Label))
                   : Visit(new BoundNopStatement(conditionalGotoStatement.Syntax));
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

        var syntax = (DoWhileStatementSyntax)doWhileStatement.Syntax;
        var startLabel = GenerateNextLabel();
        var result = Block(syntax,
                           Label(syntax.DoKeyword, startLabel),
                           doWhileStatement.Body,
                           Label(syntax.DoKeyword, doWhileStatement.ContinueLabel),
                           GotoTrue(syntax.Condition, startLabel, doWhileStatement.Condition),
                           Label(syntax.LastToken, doWhileStatement.BreakLabel));
        return Visit(result);
    }
    protected override BoundNode VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        var rewritten = (BoundStatement)base.VisitBoundExpressionStatement(expressionStatement);
        return SequencePoint(rewritten, rewritten.Syntax.Location);
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

        var syntax = (ForStatementSyntax)forStatement.Syntax;


        var upperVariableDeclaration = ConstantDeclaration(syntax, "<upperBound>", forStatement.UpperBound);
        var result = Block(syntax,
                           VariableDeclaration(syntax, forStatement.Variable, forStatement.LowerBound),
                           upperVariableDeclaration,
                           While(syntax,
                                 LessOrEqual(syntax.ToKeyword,
                                             Variable(syntax.IdentifierToken, forStatement.Variable),
                                             Variable(syntax.UpperBound, upperVariableDeclaration.Variable)),
                                 Block(syntax.Body,
                                       forStatement.Body,
                                       Label(syntax.ForKeyword, forStatement.ContinueLabel),
                                       Increment(syntax.ToKeyword, Variable(syntax.IdentifierToken, forStatement.Variable))),
                                 forStatement.BreakLabel,
                                 new($"<lowcontinue{labelCount++}>"))
        );
        return Visit(result);
    }
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
    protected override BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        var rewritten = (BoundStatement)base.VisitBoundVariableDeclarationStatement(variableDeclarationStatement);
        return SequencePoint(rewritten, rewritten.Syntax.Location);
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
                           Label(syntax.GetChild(0), whileStatement.ContinueLabel),
                           GotoFalse(syntax.GetChild(0), whileStatement.BreakLabel, whileStatement.Condition),
                           whileStatement.Body,
                           Goto(syntax.GetChild(0), whileStatement.ContinueLabel),
                           Label(syntax.LastToken, whileStatement.BreakLabel));
        return Visit(result);
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
    static BoundBlockStatement RemoveDeadCode(BoundBlockStatement statement, ControlFlowGraph controlFlowGraph, DiagnosticBag diagnostics)
    {
        var reachableStatements = controlFlowGraph.Blocks.SelectMany(block => block.Statements).ToHashSet();
        if (reachableStatements.Count == statement.Statements.Length) return statement;
        var builder = statement.Statements.ToBuilder();
        var unreachableReported = false;
        for (int i = builder.Count - 1; i >= 0; i--)
        {
            var current = builder[i];
            if (reachableStatements.Contains(current)) continue;
            builder.RemoveAt(i);
            // these kinds are injected by the lowerer, so these are not the relevant nodes.
            if (current.Kind is BoundNodeKind.GotoStatement or BoundNodeKind.ConditionalGotoStatement or BoundNodeKind.LabelStatement) continue;
            if (unreachableReported) continue;
            unreachableReported = true;
            diagnostics.ReportUnreachableCode(current.Syntax.Location);
        }

        return new (statement.Syntax, builder.ToImmutable());
    }
    public static BoundBlockStatement Lower(BoundStatement statement, FunctionSymbol? containingFunction, DiagnosticBag diagnostics)
    {
        var flat = Flatten((BoundStatement)new Lowerer().Visit(statement), containingFunction);
        var controlFlowGraph = ControlFlowGraph.Create(flat);
        if (containingFunction is not null && containingFunction.ReturnType != TypeSymbol.Void && !controlFlowGraph.AllPathsReturn())
            diagnostics.ReportNotAllPathsReturn(containingFunction);
        return RemoveDeadCode(flat, controlFlowGraph, diagnostics);
    }
}
