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
    BoundSequencePointStatement? currentSequencePointStatement;
    

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
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        var startLabel = GenerateNextLabel();
        builder.Add(Label(syntax.DoKeyword, startLabel));
        AddLoopBody(builder, doWhileStatement.Body);
        builder.Add(Label(syntax.DoKeyword, doWhileStatement.ContinueLabel));
        builder.Add(SequencePoint(GotoTrue(syntax.Condition, startLabel, doWhileStatement.Condition),
                                         syntax.WhileKeyword.Location + syntax.Condition.Location));
        builder.Add(Label(syntax.LastToken, doWhileStatement.BreakLabel));
        return Visit(Block(syntax, builder.ToImmutable()));
    }
    protected override BoundNode VisitBoundExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        var rewritten = (BoundStatement)base.VisitBoundExpressionStatement(expressionStatement);
        return currentSequencePointStatement?.Statement != expressionStatement ? SequencePoint(rewritten, rewritten.Syntax.Location) : rewritten;
    }
    protected override BoundNode VisitBoundForStatement(BoundForStatement forStatement)
    {
        /*
         * Source statement:
         *   for <var> = <lower> to <upper>
         *     <body>
         *
         * Target statement:
         *
         *  var <var> = <lower>
         *  var <tmp> = <upper>
         *  <start>:
         *  gotofalse <var> <= <tmp> -> <break>
         *  <body>
         *  <continue>:
         *  <var>++
         *  goto <start>
         *  <break>:
         */

        var syntax = (ForStatementSyntax)forStatement.Syntax;

        var startLabel = new BoundLabel("<start>");
        var lowerVariableDeclaration =VariableDeclaration(syntax.LowerBound, forStatement.Variable, forStatement.LowerBound);
        var upperVariableDeclaration = ConstantDeclaration(syntax.UpperBound, "<upperBound>", forStatement.UpperBound);
        var checkStatement = GotoFalse(syntax.ToKeyword, forStatement.BreakLabel, 
                                       LessOrEqual(syntax.ToKeyword, 
                                                   Variable(syntax.IdentifierToken, forStatement.Variable),
                                                   Variable(syntax.UpperBound, upperVariableDeclaration.Variable)));
        var increment = Increment(syntax.ToKeyword, Variable(syntax.IdentifierToken, forStatement.Variable));

        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        builder.Add(SequencePoint(lowerVariableDeclaration, syntax.IdentifierToken.Location + syntax.EqualsToken.Location + syntax.LowerBound.Location));
        builder.Add(SequencePoint(upperVariableDeclaration, syntax.ToKeyword.Location + syntax.UpperBound.Location));
        builder.Add(Label(syntax.ToKeyword, startLabel));
        builder.Add(SequencePoint(checkStatement, syntax.UpperBound.Location));

        AddLoopBody(builder, forStatement.Body);

        builder.Add(Label(syntax.ToKeyword, forStatement.ContinueLabel));
        builder.Add(SequencePoint(increment, syntax.ToKeyword.Location));
        builder.Add(Goto(syntax.ToKeyword, startLabel));
        builder.Add(Label(syntax.LastToken, forStatement.BreakLabel));

        return Visit(Block(syntax, builder.ToImmutable()));
    }
    protected override BoundNode VisitBoundIfStatement(BoundIfStatement ifStatement)
    {
        BoundStatement result;

        var syntax = (IfStatementSyntax)ifStatement.Syntax;
        var iflocation = syntax.IfKeyword.Location + syntax.Condition.Location;

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
        return currentSequencePointStatement?.Statement != variableDeclarationStatement ? SequencePoint(rewritten, rewritten.Syntax.Location) : rewritten;
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

        var syntax = (WhileStatementSyntax)whileStatement.Syntax;
        var debugLocation = syntax.WhileKeyword.Location + syntax.Condition.Location;
        BoundStatement condition = SequencePoint(GotoFalse(syntax.GetChild(0), whileStatement.BreakLabel, whileStatement.Condition), debugLocation);

        var builder = ImmutableArray.CreateBuilder<BoundStatement>();

        builder.Add(Label(syntax.GetChild(0), whileStatement.ContinueLabel));
        builder.Add(condition);
        AddLoopBody(builder, whileStatement.Body);
        builder.Add(Goto(syntax.LastToken, whileStatement.ContinueLabel));
        builder.Add(Label(syntax.LastToken, whileStatement.BreakLabel));

        return Visit(Block(syntax, builder.ToImmutable()));
    }
    protected override BoundNode VisitBoundReturnStatement(BoundReturnStatement node) => 
        // Add sequence points only for real return statements that were not injected.
        node.Syntax.Kind == SyntaxKind.ReturnStatement ? SequencePoint(node, node.Syntax.Location) : node;
    protected override BoundNode VisitBoundSequencePointStatement(BoundSequencePointStatement node)
    {
        var seq = currentSequencePointStatement;
        currentSequencePointStatement = node;
        var result = base.VisitBoundSequencePointStatement(node);
        currentSequencePointStatement = seq;
        return result;
    }

    static void AddLoopBody(ImmutableArray<BoundStatement>.Builder builder, BoundStatement body)
    {
        if (body.Kind != BoundNodeKind.BlockStatement)
        {
            builder.Add(body);
            return;
        }
        var block = (BoundBlockStatement)body;
        var openBraceToken = block.Syntax.Kind == SyntaxKind.BlockStatement ? ((BlockStatementSyntax)block.Syntax).OpenBraceToken : null;
        var closedBraceToken = block.Syntax.Kind == SyntaxKind.BlockStatement ? ((BlockStatementSyntax)block.Syntax).ClosedBraceToken : null;
        if (openBraceToken is not null)
        {
            builder.Add(new BoundBeginScopeStatement(openBraceToken));
            builder.Add(SequencePoint(Nop(openBraceToken), openBraceToken.Location));
        }

        builder.AddRange(block.Statements);

        if (closedBraceToken is not null)
        {
            builder.Add(SequencePoint(Nop(closedBraceToken), closedBraceToken.Location));
            builder.Add(new BoundEndScopeStatement(closedBraceToken));
        }
    }

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
                {
                    stack.Push(new BoundEndScopeStatement(statement.Syntax));
                    stack.Push(SequencePoint(Nop(blockSyntax.ClosedBraceToken), blockSyntax.ClosedBraceToken.Location));
                }

                foreach (var s in statements.Reverse())
                    stack.Push(s);

                if (blockSyntax is { })
                {
                    stack.Push(SequencePoint(Nop(blockSyntax.OpenBraceToken), blockSyntax.OpenBraceToken.Location));
                    stack.Push(new BoundBeginScopeStatement(statement.Syntax));
                }
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
            s.Kind != BoundNodeKind.BeginScopeStatement &&
            s.Kind != BoundNodeKind.EndScopeStatement &&
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
