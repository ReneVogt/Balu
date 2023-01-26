using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Binding;
using Balu.Diagnostics;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using static Balu.Binding.BoundNodeFactory;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeRewriter
{
    int labelCount;

    BoundLabel GenerateNextLabel() => new($"Label{labelCount++}");

    protected override BoundNode VisitBoundGotoStatement(BoundGotoStatement node)
    {
        if (node.Syntax.Kind != SyntaxKind.BreakStatement && node.Syntax.Kind != SyntaxKind.ContinueStatement) return node;
        return SequencePoint(node, node.Syntax.Location);
    }
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
                           SequencePoint(GotoTrue(syntax.Condition, startLabel, doWhileStatement.Condition),
                                         new(syntax.SyntaxTree.Text,
                                             syntax.WhileKeyword.Span with { Length = syntax.Condition.Span.End - syntax.WhileKeyword.Span.Start })),
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

        var syntax = (IfStatementSyntax)ifStatement.Syntax;
        var iflocation = new TextLocation(syntax.SyntaxTree.Text,
                                          syntax.IfKeyword.Span with { Length = syntax.Condition.Span.End - syntax.IfKeyword.Span.Start });

        if (ifStatement.ElseStatement is null)
        {
            /*
             *   if <condition>         GotoIfFalse <condition> <end>
             *      <then>              <then>
             *                          end:
             */
            var endLabel = GenerateNextLabel();
            result = Block(syntax,
                           SequencePoint(GotoFalse(syntax.IfKeyword, endLabel, ifStatement.Condition), iflocation),
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
                           SequencePoint(GotoFalse(syntax, elseLabel, ifStatement.Condition), iflocation),
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
    protected override BoundNode VisitBoundReturnStatement(BoundReturnStatement node) => 
        // Add sequence points only for real return statements that were not injected.
        node.Syntax.Kind == SyntaxKind.ReturnStatement ? SequencePoint(node, node.Syntax.Location) : node;
    static BoundBlockStatement Flatten(BoundStatement statement)
    {
        var resultBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
        Stack<BoundStatement> stack = new();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current is BoundBlockStatement { Statements: var statements , Syntax: var syntax})
            {
                BlockStatementSyntax? blockSyntax = syntax.Kind == SyntaxKind.BlockStatement && current != statement ? (BlockStatementSyntax)syntax : null;
                if (blockSyntax is { })
                    stack.Push(SequencePoint(Nop(blockSyntax.ClosedBraceToken), blockSyntax.ClosedBraceToken.Location));
                foreach (var s in statements.Reverse())
                    stack.Push(s);
                if (blockSyntax is { })
                    stack.Push(SequencePoint(Nop(blockSyntax.OpenBraceToken), blockSyntax.OpenBraceToken.Location));
            }
            else
                resultBuilder.Add(current);
        }

        return new (statement.Syntax, resultBuilder.ToImmutable());
    }
    static BoundBlockStatement RemoveDebris(BoundBlockStatement block)
    {
        var builder = block.Statements.ToBuilder();

        // remove nops
        for (int i = 0; i < builder.Count; i++)
        {
            if (builder[i].Kind == BoundNodeKind.NopStatement)
                builder.RemoveAt(i);
        }

        // collapse redundant gotos
        for (int i = 0; i < builder.Count-1; i++)
        {
            var next = builder[i + 1].UnwrapSequencePoint();
            if (next.Kind != BoundNodeKind.LabelStatement) continue;
            var label = ((BoundLabelStatement)next).Label;

            var current = builder[i].UnwrapSequencePoint();
            if (current.Kind == BoundNodeKind.SequencePointStatement)
                current = ((BoundSequencePointStatement)current).Statement;
            if ((current.Kind != BoundNodeKind.GotoStatement || ((BoundGotoStatement)current).Label != label) &&
                (current.Kind != BoundNodeKind.ConditionalGotoStatement|| ((BoundConditionalGotoStatement)current).Label != label)) continue;

            builder.RemoveAt(i + 1);
            builder.RemoveAt(i);
            i -= 2;
        }


        // remove unused lables
        var usedLabels = new HashSet<BoundLabel>(builder.Select(s => s.UnwrapSequencePoint())
                                                        .Where(s => s.Kind == BoundNodeKind.GotoStatement)
                                                        .Select(s => ((BoundGotoStatement)s).Label)
                                                        .Concat(builder.Select(s => s.UnwrapSequencePoint())
                                                                       .Where(s => s.Kind == BoundNodeKind.ConditionalGotoStatement)
                                                                       .Select(s => ((BoundConditionalGotoStatement)s).Label)));
        for (int i = 0; i < builder.Count; i++)
        {
            var current = builder[i].UnwrapSequencePoint();
            if (current.Kind == BoundNodeKind.LabelStatement && !usedLabels.Contains(((BoundLabelStatement)current).Label))
                builder.RemoveAt(i--);
        }

        return Block(block.Syntax, builder.ToImmutable());
    }
    static BoundBlockStatement RemoveDeadCode(BoundBlockStatement statement, ControlFlowGraph controlFlowGraph, DiagnosticBag diagnostics)
    {
        var unreachableStatements = new HashSet<BoundStatement>(
            controlFlowGraph.DeadBlocks.SelectMany(deadBlock => deadBlock.Statements.Where(IsNotInjected)));
        if (unreachableStatements.Count == 0) return statement;
        var builder = statement.Statements.ToBuilder();
        for (int i = 0; i < builder.Count; i++)
        {
            var current = builder[i];
            if (unreachableStatements.Contains(current))
                builder.RemoveAt(i--);
        }

        var list = unreachableStatements.ToList();
        while (list.Count > 0)
        {
            var consecutive = unreachableStatements.Where(s => s.Syntax.FullSpan.OverlapsWithOrTouches(list[0].Syntax.FullSpan)).ToList();
            foreach (var s in consecutive) list.Remove(s);
            var start = consecutive.Min(s => s.Syntax.Span.Start);
            var end = consecutive.Max(s => s.Syntax.Span.End);
            diagnostics.ReportUnreachableCode(new(consecutive[0].Syntax.SyntaxTree.Text, new(start, end - start)));
        }

        return new (statement.Syntax, builder.ToImmutable());

        static bool IsNotInjected(BoundStatement s) => 
            s.Kind != BoundNodeKind.GotoStatement && 
            s.Kind != BoundNodeKind.ConditionalGotoStatement && 
            s.Kind != BoundNodeKind.LabelStatement &&
            s.Kind != BoundNodeKind.NopStatement && 
            (s.Kind != BoundNodeKind.ReturnStatement || s.Syntax.Kind == SyntaxKind.ReturnStatement) &&
            (s.Kind != BoundNodeKind.SequencePointStatement || IsNotInjected(((BoundSequencePointStatement)s).Statement));
    }
    static BoundBlockStatement AddMissingReturn(BoundBlockStatement block, FunctionSymbol? containingFunction)
    {
        if (containingFunction?.ReturnType != TypeSymbol.Void || block.Statements.Length > 0 && !CanFallThrough(block.Statements.Last().UnwrapSequencePoint()))
            return block;

        return Block(block.Syntax, block.Statements.Add(new BoundReturnStatement(block.Syntax, null)));

        static bool CanFallThrough(BoundStatement lastStatement) =>
            lastStatement.Kind != BoundNodeKind.ReturnStatement &&
            lastStatement.Kind != BoundNodeKind.GotoStatement;
    }
    public static BoundBlockStatement Lower(BoundStatement statement, FunctionSymbol? containingFunction, DiagnosticBag diagnostics)
    {
        var lowered = (BoundStatement)new Lowerer().Visit(statement);
        var resultingBlock = Flatten(lowered);

        var controlFlowGraph = ControlFlowGraph.Create(resultingBlock);
        if (containingFunction is not null && containingFunction.ReturnType != TypeSymbol.Void && !controlFlowGraph.AllPathsReturn())
            diagnostics.ReportNotAllPathsReturn(containingFunction);

        resultingBlock = RemoveDeadCode(resultingBlock, controlFlowGraph, diagnostics);
        resultingBlock = RemoveDebris(resultingBlock);
        resultingBlock = AddMissingReturn(resultingBlock, containingFunction);
        return resultingBlock;
    }
}
