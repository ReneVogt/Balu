﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Balu.Binding;
using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Lowering;

sealed class ControlFlowGraph
{
    public sealed class Block
    {
        public bool IsStart { get; }
        public bool IsEnd { get; }
        public List<BoundStatement> Statements { get; } = [];
        public List<Edge> Incoming { get; } = [];
        public List<Edge> Outgoing { get; } = [];

        public Block() { }
        public Block(bool isStart)
        {
            IsStart = isStart;
            IsEnd = !isStart;
        }

        public override string ToString()
        {
            if (IsStart) return "<Start>";
            if (IsEnd) return "<End>";
            return string.Join(Environment.NewLine, Statements);
        }
    }

    public sealed class Edge(ControlFlowGraph.Block from, ControlFlowGraph.Block to, BoundExpression? condition)
    {
        public Block From { get; } = from;
        public Block To { get; } = to;
        public BoundExpression? Condition { get; } = condition;

        public override string ToString() => Condition?.ToString() ?? string.Empty;
    }

    sealed class BlockBuilder
    {
        readonly List<BoundStatement> statements = [];
        readonly List<Block> blocks = [];

        public List<Block> Build(BoundBlockStatement blockStatement)
        {
            blocks.Clear();
            statements.Clear();
            foreach (var statement in blockStatement.Statements)
            {
                var unwrapped = statement.UnwrapSequencePoint();
                switch (unwrapped.Kind)
                {
                    case BoundNodeKind.VariableDeclarationStatement:
                    case BoundNodeKind.ExpressionStatement:
                    case BoundNodeKind.NopStatement:
                    case BoundNodeKind.BeginScopeStatement:
                    case BoundNodeKind.EndScopeStatement:
                        statements.Add(statement);
                        break;
                    case BoundNodeKind.LabelStatement:
                        StartBlock();
                        statements.Add(statement);
                        break;
                    case BoundNodeKind.ReturnStatement:
                    case BoundNodeKind.GotoStatement:
                    case BoundNodeKind.ConditionalGotoStatement:
                        statements.Add(statement);
                        StartBlock();
                        break;
                    default:
                        throw new ControlFlowException($"Unknown statement {statement.Kind}.");
                }
            }

            EndBlock();

            return blocks.ToList();
        }

        void StartBlock()
        {
            EndBlock();
        }
        void EndBlock()
        {
            if (statements.Count == 0) return;
            var block = new Block();
            block.Statements.AddRange(statements);
            blocks.Add(block);
            statements.Clear();
        }
    }

    sealed class GraphBuilder
    {
        readonly List<Edge> edges = [];

        public ControlFlowGraph Build(List<Block> blocks)
        {
            Block start = new(true);
            Block end = new(false);

            if (blocks.Count == 0)
                Connect(start, end);
            else
                Connect(start, blocks[0]);

            var blockFromLabel = blocks.SelectMany(block => block.Statements.Where(statement => statement.Kind == BoundNodeKind.LabelStatement)
                                                                 .Select(statement => (((BoundLabelStatement)statement).Label, block)))
                                       .ToDictionary(x => x.Label, x => x.block);

            for (int blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
            {
                var block = blocks[blockIndex];
                var nextBlock = blockIndex < blocks.Count - 1 ? blocks[blockIndex + 1] : end;

                for (int statementIndex = 0; statementIndex < block.Statements.Count; statementIndex++)
                {
                    var statement = block.Statements[statementIndex];
                    var unwrapped = statement.UnwrapSequencePoint();
                    var isLast = statementIndex == block.Statements.Count - 1;
                    switch (unwrapped.Kind)
                    {
                        case BoundNodeKind.VariableDeclarationStatement:
                        case BoundNodeKind.ExpressionStatement:
                        case BoundNodeKind.LabelStatement:
                        case BoundNodeKind.NopStatement:
                        case BoundNodeKind.BeginScopeStatement:
                        case BoundNodeKind.EndScopeStatement:
                            if (isLast) Connect(block, nextBlock);
                            break;
                        case BoundNodeKind.ReturnStatement:
                            Connect(block, end);
                            break;
                        case BoundNodeKind.GotoStatement:
                            Connect(block, blockFromLabel[((BoundGotoStatement)unwrapped).Label]);
                            break;
                        case BoundNodeKind.ConditionalGotoStatement:
                            var cgs = (BoundConditionalGotoStatement)unwrapped;
                            var negated = new BoundUnaryExpression(cgs.Condition.Syntax, BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Boolean)!, cgs.Condition);
                            var jumpCondition = cgs.JumpIfTrue ? cgs.Condition : negated;
                            var nextCondition = cgs.JumpIfTrue ? negated : cgs.Condition;
                            Connect(block, blockFromLabel[cgs.Label], jumpCondition);
                            Connect(block, nextBlock, nextCondition);
                            break;
                        default:
                            throw new ControlFlowException($"Unknown statement {statement.Kind}.");
                    }
                }
            }

            bool removed;
            var deadBlockBuilder = ImmutableArray.CreateBuilder<Block>();
            do
            {
                var toRemove = blocks.Where(block => block.Incoming.Count == 0).ToArray();
                deadBlockBuilder.AddRange(toRemove);
                removed = toRemove.Length > 0;

                foreach (var block in toRemove)
                {
                    blocks.Remove(block);
                    foreach (var incomingEdge in block.Incoming)
                    {
                        incomingEdge.From.Outgoing.Remove(incomingEdge);
                        edges.Remove(incomingEdge);
                    }

                    foreach (var outgoingEdge in block.Outgoing)
                    {
                        outgoingEdge.To.Incoming.Remove(outgoingEdge);
                        edges.Remove(outgoingEdge);
                    }
                }
            } while (removed);

            blocks.Insert(0, start);
            blocks.Add(end);

            return new(start, end, blocks, edges, deadBlockBuilder.ToImmutable());
        }

        void Connect(Block from, Block to, BoundExpression? condition = null)
        {
            var value = EvaluateCondition(condition);
            if (value == true)
                condition = null;
            if (value == false) return;
            var edge = new Edge(from, to, condition);
            from.Outgoing.Add(edge);
            to.Incoming.Add(edge);
            edges.Add(edge);
        }
        static bool? EvaluateCondition(BoundExpression? condition)
        {
            if (condition is null) return null;
            switch (condition.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return (bool)((BoundLiteralExpression)condition).Value;
                case BoundNodeKind.UnaryExpression:
                    {
                        var unary = (BoundUnaryExpression)condition;
                        var value = EvaluateCondition(unary.Operand);
                        if (value is null) return null;
                        return unary.Operator.OperatorKind == BoundUnaryOperatorKind.LogicalNegation
                        ? !value.Value
                        : null;
                    }
                default: return null;
            }
        }
    }

    public Block Start { get; }
    public Block End { get; }
    public ImmutableArray<Block> Blocks { get; }
    public ImmutableArray<Edge> Edges { get; }
    public ImmutableArray<Block> DeadBlocks { get; }

    ControlFlowGraph(Block start, Block end, List<Block> blocks, List<Edge> edges, ImmutableArray<Block> deadBlocks)
    {
        Start = start;
        End = end;
        Blocks = blocks.ToImmutableArray();
        Edges = edges.ToImmutableArray();
        DeadBlocks = deadBlocks;
    }

    public void WriteTo(TextWriter writer)
    {
        writer.WriteLine("digraph G {");
        var blockIds = Blocks.Select((block, index) => (block, index))
                             .ToDictionary(x => x.block, x => $"N{x.index}");

        foreach (var block in Blocks)
        {
            var id = blockIds[block];
            var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
            writer.WriteLine($"    {id} [label = {label}, shape = box]");
        }
        foreach (var edge in Edges)
        {
            var fromId = blockIds[edge.From];
            var toId = blockIds[edge.To];
            var label = Quote(edge.Condition?.ToString() ?? string.Empty);
            writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
        }
        writer.WriteLine("}");

        static string Quote(string s) => $"\"{s.Replace("\"", "\\\"")}\"";
    }

    public static ControlFlowGraph Create(BoundBlockStatement statement)
    {
        var blocks = new BlockBuilder().Build(statement);
        return new GraphBuilder().Build(blocks);
    }

    public bool AllPathsReturn() => End.Incoming
                                       .All(incoming =>
                                                incoming.From.Statements.LastOrDefault()?.UnwrapSequencePoint().Kind ==
                                                BoundNodeKind.ReturnStatement);
}