﻿using System;
using System.CodeDom.Compiler;
using System.IO;    
using Balu.Binding;
using Balu.Symbols;
using Balu.Syntax;

#pragma warning disable CA1001 // The BoundTreeVisitor does not hold the dispose ownership of the textwriter.

namespace Balu.Visualization;

sealed class BoundTreeWriter : BoundTreeVisitor
{
    const string TABSTRING = "  ";
    readonly IndentedTextWriter writer;

    BoundTreeWriter(IndentedTextWriter writer) => this.writer = writer;

    protected override BoundNode VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression)
    {
        writer.WriteIdentifier(assignmentExpression.Symbol.Name);
        writer.WritePunctuation(" = ");
        Visit(assignmentExpression.Expression);
        return assignmentExpression;
    }

    protected override BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        var op = binaryExpression.Operator.SyntaxKind;
        var precedence = op.BinaryOperatorPrecedence();
        WriteNestedExpression(binaryExpression.Left, precedence);
        writer.WritePunctuation(" " + op.GetText()! + " ");
        WriteNestedExpression(binaryExpression.Right, precedence);
        return binaryExpression;
    }

    protected override BoundNode VisitBoundBlockStatement(BoundBlockStatement blockStatement)
    {
        writer.WritePunctuation("{");
        writer.WriteLine();
        writer.Indent++;
        foreach (var statement in blockStatement.Statements)
        {
            Visit(statement);
            writer.WriteLine();
        }
        writer.Indent--;
        writer.WritePunctuation("}");
        return blockStatement;
    }

    protected override BoundNode VisitBoundCallExpression(BoundCallExpression callExpression)
    {
        writer.WriteIdentifier(callExpression.Function.Name);
        writer.WritePunctuation("(");
        for (int i = 0; i < callExpression.Arguments.Length; i++)
        {
            if (i > 0) writer.WritePunctuation(", ");
            Visit(callExpression.Arguments[i]);
        }
        writer.WritePunctuation(")");
        return callExpression;
    }

    protected override BoundNode VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(conditionalGotoStatement.Label.Name);
        writer.WriteKeyword(conditionalGotoStatement.JumpIfTrue ? " if " : " unless ");
        Visit(conditionalGotoStatement.Condition);
        return conditionalGotoStatement;
    }

    protected override BoundNode VisitBoundConversionExpression(BoundConversionExpression conversionExpression)
    {
        writer.WriteIdentifier(conversionExpression.Type.Name);
        writer.WritePunctuation("(");
        Visit(conversionExpression.Expression);
        writer.WritePunctuation(")");
        return conversionExpression;
    }

    protected override BoundNode VisitBoundDoWhileStatement(BoundDoWhileStatement doWhileStatement)
    {
        writer.WriteKeyword("do ");
        writer.WriteLine();
        WriteNestedStatement(doWhileStatement.Body);
        Visit(doWhileStatement.Condition);
        return doWhileStatement;
    }

    protected override BoundNode VisitBoundErrorExpression(BoundErrorExpression errorExpression)
    {
        writer.WriteKeyword("?");
        return errorExpression;
    }

    protected override BoundNode VisitBoundForStatement(BoundForStatement forStatement)
    {
        writer.WriteKeyword("for ");
        writer.WriteIdentifier(forStatement.Variable.Name);
        writer.WritePunctuation(" = ");
        Visit(forStatement.LowerBound);
        writer.WriteKeyword(" to ");
        Visit(forStatement.UpperBound);
        writer.WriteLine();
        WriteNestedStatement(forStatement.Body);
        return forStatement;
    }

    protected override BoundNode VisitBoundGotoStatement(BoundGotoStatement gotoStatement)
    {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(gotoStatement.Label.Name);
        return gotoStatement;
    }

    protected override BoundNode VisitBoundIfStatement(BoundIfStatement ifStatement)
    {
        writer.WriteKeyword("if ");
        Visit(ifStatement.Condition);
        writer.WriteLine();
        WriteNestedStatement(ifStatement.ThenStatement);
        writer.WriteLine();
        if (ifStatement.ElseStatement is not null)
        {
            writer.WriteKeyword("else");
            writer.WriteLine();
            WriteNestedStatement(ifStatement.ElseStatement);
        }
        return ifStatement;
    }

    protected override BoundNode VisitBoundLabelStatement(BoundLabelStatement labelStatement)
    {
        bool indent = writer.Indent > 0;
        if (indent) writer.Indent--;
        writer.WritePunctuation(labelStatement.Label.Name + ":");
        if (indent) writer.Indent++;
        return labelStatement;
    }

    protected override BoundNode VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        var value = literalExpression.Value.ToString()!;
        if (literalExpression.Type == TypeSymbol.Boolean)
            writer.WriteKeyword(value);
        else if (literalExpression.Type == TypeSymbol.Integer)
            writer.WriteNumber(value);
        else if (literalExpression.Type == TypeSymbol.String)
            writer.WriteString($"\"{value.EscapeString()}\"");
        else throw new ArgumentException($"Unsupported literal expression type '{literalExpression.Type}'.");
        return literalExpression;
    }

    protected override BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        var op = unaryExpression.Operator.SyntaxKind;
        writer.WritePunctuation(op.GetText()!);
        WriteNestedExpression(unaryExpression.Operand, op.UnaryOperatorPrecedence());
        return unaryExpression;
    }

    protected override BoundNode VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        writer.WriteKeyword(variableDeclarationStatement.Variable.ReadOnly ? "let " : "var ");
        writer.WriteIdentifier(variableDeclarationStatement.Variable.Name);
        writer.WritePunctuation(" = ");
        variableDeclarationStatement.Accept(this);
        return variableDeclarationStatement;
    }

    protected override BoundNode VisitBoundVariableExpression(BoundVariableExpression variableExpression)
    {
        writer.WriteIdentifier(variableExpression.Variable.Name);
        return variableExpression;
    }

    protected override BoundNode VisitBoundWhileStatement(BoundWhileStatement whileStatement)
    {
        writer.WriteKeyword("while ");
        Visit(whileStatement.Condition);
        writer.WriteLine();
        WriteNestedStatement(whileStatement.Body);
        return whileStatement;
    }

    void WriteNestedStatement(BoundStatement statement)
    {
        if (statement is BoundBlockStatement block)
        {
            writer.WritePunctuation("{");
            writer.WriteLine();
            writer.Indent++;
            foreach (var child in block.Statements)
            {
                Visit(child);
                writer.WriteLine();
            }
            writer.Indent--;
            writer.WritePunctuation("}");
            return;
        }
        writer.Indent++;
        Visit(statement);
        writer.Indent--;
    }
    void WriteNestedExpression(BoundExpression expression, int parentPrecedence)
    {
        switch (expression.Kind)
        {
            case BoundNodeKind.UnaryExpression:
                WriteNestedExpression(expression, parentPrecedence, ((BoundUnaryExpression)expression).Operator.SyntaxKind.UnaryOperatorPrecedence());
                break;
            case BoundNodeKind.BinaryExpression:
                WriteNestedExpression(expression, parentPrecedence, ((BoundBinaryExpression)expression).Operator.SyntaxKind.BinaryOperatorPrecedence());
                break;
            default: 
                Visit(expression);
                break;
        }

    }
    void WriteNestedExpression(BoundExpression expression, int parentPrecedence, int currentPrecedence)
    {
        bool parenthesis = parentPrecedence >= currentPrecedence;

        if (parenthesis)
            writer.WritePunctuation("(");

        Visit(expression);

        if (parenthesis)
            writer.WritePunctuation(")");
    }

    static void WriteFunction(IndentedTextWriter writer, FunctionSymbol function, BoundBlockStatement body)
    {
        writer.WriteKeyword("function ");
        writer.WriteIdentifier(function.Name);
        writer.WritePunctuation("(");
        for (int i = 0; i < function.Parameters.Length; i++)
        {
            if (i > 0) writer.WritePunctuation(", ");
            writer.WriteIdentifier(function.Parameters[i].Name);
            writer.WritePunctuation(" : ");
            writer.WriteIdentifier(function.Parameters[i].Type.Name);
        }
        writer.WritePunctuation(")");
        writer.WriteLine();
        Print(body, writer);
        writer.WriteLine();
    }

    public static void Print(BoundNode boundNode, TextWriter textWriter)
    {
        var indentedTextWriter = textWriter as IndentedTextWriter ?? new IndentedTextWriter(textWriter, TABSTRING);
        new BoundTreeWriter(indentedTextWriter).Visit(boundNode);
    }
    public static void Print(BoundProgram boundProgram, TextWriter textWriter)
    {
        var indentedTextWriter = textWriter as IndentedTextWriter ?? new IndentedTextWriter(textWriter, TABSTRING);
        foreach (var function in boundProgram.Functions)
            WriteFunction(indentedTextWriter, function.Key, function.Value);
        Print(boundProgram.GlobalScope.Statement, indentedTextWriter);
        indentedTextWriter.WriteLine();
    }
}
