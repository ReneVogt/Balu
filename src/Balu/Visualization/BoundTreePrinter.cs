using System;
using System.CodeDom.Compiler;
using System.IO;    
using Balu.Binding;
using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Visualization;

sealed class BoundTreePrinter : BoundTreeVisitor
{
    const string TABSTRING = "  ";
    readonly IndentedTextWriter writer;

    BoundTreePrinter(IndentedTextWriter writer) => this.writer = writer;

    protected override void VisitBoundAssignmentExpression(BoundAssignmentExpression assignmentExpression)
    {
        writer.WriteIdentifier(assignmentExpression.Symbol.Name);
        writer.WriteSpace();
        writer.WritePunctuation(SyntaxKind.EqualsToken.GetText());
        writer.WriteSpace();
        Visit(assignmentExpression.Expression);
    }

    protected override void VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        var op = binaryExpression.Operator.SyntaxKind;
        var precedence = op.BinaryOperatorPrecedence();
        WriteNestedExpression(binaryExpression.Left, precedence);
        writer.WriteSpace();
        writer.WritePunctuation(op.GetText());
        writer.WriteSpace();
        WriteNestedExpression(binaryExpression.Right, precedence);
    }

    protected override void VisitBoundBlockStatement(BoundBlockStatement blockStatement)
    {
        writer.WritePunctuation(SyntaxKind.OpenBraceToken.GetText());
        writer.WriteLine();
        writer.Indent++;
        foreach (var statement in blockStatement.Statements)
        {
            Visit(statement);
            writer.WriteLine();
        }
        writer.Indent--;
        writer.WritePunctuation(SyntaxKind.ClosedBraceToken.GetText());
    }

    protected override void VisitBoundCallExpression(BoundCallExpression callExpression)
    {
        writer.WriteIdentifier(callExpression.Function.Name);
        writer.WritePunctuation(SyntaxKind.OpenParenthesisToken.GetText());
        for (int i = 0; i < callExpression.Arguments.Length; i++)
        {
            if (i > 0)
            {
                writer.WritePunctuation(SyntaxKind.CommaToken.GetText());
                writer.WriteSpace();
            }
            Visit(callExpression.Arguments[i]);
        }
        writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken.GetText());
    }

    protected override void VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        writer.WriteKeyword("goto");
        writer.WriteSpace();
        writer.WriteIdentifier(conditionalGotoStatement.Label.Name);
        writer.WriteSpace();
        writer.WriteKeyword(conditionalGotoStatement.JumpIfTrue ? "if" : "unless");
        writer.WriteSpace();
        Visit(conditionalGotoStatement.Condition);
    }

    protected override void VisitBoundConversionExpression(BoundConversionExpression conversionExpression)
    {
        writer.WriteIdentifier(conversionExpression.Type.Name);
        writer.WritePunctuation(SyntaxKind.OpenParenthesisToken.GetText());
        Visit(conversionExpression.Expression);
        writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken.GetText());
    }

    protected override void VisitBoundDoWhileStatement(BoundDoWhileStatement doWhileStatement)
    {
        writer.WriteKeyword(SyntaxKind.DoKeyword.GetText());
        writer.WriteLine();
        WriteNestedStatement(doWhileStatement.Body);
        if (doWhileStatement.Body.Kind == BoundNodeKind.BlockStatement) writer.WriteSpace();
        else writer.WriteLine();
        writer.WriteKeyword(SyntaxKind.WhileKeyword.GetText());
        writer.WriteSpace();
        Visit(doWhileStatement.Condition);
    }

    protected override void VisitBoundErrorExpression(BoundErrorExpression errorExpression)
    {
        writer.WriteKeyword("?");
    }

    protected override void VisitBoundForStatement(BoundForStatement forStatement)
    {
        writer.WriteKeyword(SyntaxKind.ForKeyword.GetText());
        writer.WriteSpace();
        writer.WriteIdentifier(forStatement.Variable.Name);
        writer.WriteSpace();
        writer.WritePunctuation(SyntaxKind.EqualsToken.GetText());
        writer.WriteSpace();
        Visit(forStatement.LowerBound);
        writer.WriteSpace();
        writer.WriteKeyword(SyntaxKind.ToKeyword.GetText());
        writer.WriteSpace();
        Visit(forStatement.UpperBound);
        writer.WriteLine();
        WriteNestedStatement(forStatement.Body);
    }

    protected override void VisitBoundGotoStatement(BoundGotoStatement gotoStatement)
    {
        writer.WriteKeyword("goto");
        writer.WriteSpace();
        writer.WriteIdentifier(gotoStatement.Label.Name);
    }

    protected override void VisitBoundIfStatement(BoundIfStatement ifStatement)
    {
        writer.WriteKeyword(SyntaxKind.IfKeyword.GetText());
        writer.WriteSpace();
        Visit(ifStatement.Condition);
        writer.WriteLine();
        WriteNestedStatement(ifStatement.ThenStatement);
        writer.WriteLine();
        if (ifStatement.ElseStatement is not null)
        {
            writer.WriteKeyword(SyntaxKind.ElseKeyword.GetText());
            writer.WriteLine();
            WriteNestedStatement(ifStatement.ElseStatement);
        }
    }

    protected override void VisitBoundLabelStatement(BoundLabelStatement labelStatement)
    {
        bool indent = writer.Indent > 0;
        if (indent) writer.Indent--;
        writer.WritePunctuation(labelStatement.Label.Name + SyntaxKind.ColonToken.GetText());
        if (indent) writer.Indent++;
    }

    protected override void VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        var value = literalExpression.Value.ToString()!;
        if (literalExpression.Type == TypeSymbol.Boolean)
            writer.WriteKeyword(value);
        else if (literalExpression.Type == TypeSymbol.Integer)
            writer.WriteNumber(value);
        else if (literalExpression.Type == TypeSymbol.String)
            writer.WriteString($"\"{value.EscapeString()}\"");
        else throw new ArgumentException($"Unsupported literal expression type '{literalExpression.Type}'.");
    }

    protected override void VisitBoundReturnStatement(BoundReturnStatement returnStatement)
    {
        writer.WriteKeyword(SyntaxKind.ReturnKeyword.GetText());
        if (returnStatement.Expression is not null)
        {
            writer.WriteSpace();
            Visit(returnStatement.Expression);
        }
    }

    protected override void VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        var op = unaryExpression.Operator.SyntaxKind;
        writer.WritePunctuation(op.GetText()!);
        WriteNestedExpression(unaryExpression.Operand, op.UnaryOperatorPrecedence());
    }

    protected override void VisitBoundVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        writer.WriteKeyword(variableDeclarationStatement.Variable.ReadOnly ? SyntaxKind.LetKeyword.GetText() : SyntaxKind.VarKeyword.GetText()!);
        writer.WriteSpace();
        writer.WriteIdentifier(variableDeclarationStatement.Variable.Name);
        writer.WriteSpace();
        writer.WritePunctuation(":");
        writer.WriteSpace();
        writer.WriteIdentifier(variableDeclarationStatement.Variable.Type.Name);
        writer.WriteSpace();
        writer.WritePunctuation(SyntaxKind.EqualsToken.GetText());
        writer.WriteSpace();
        variableDeclarationStatement.Accept(this);
    }

    protected override void VisitBoundVariableExpression(BoundVariableExpression variableExpression)
    {
        writer.WriteIdentifier(variableExpression.Variable.Name);
    }

    protected override void VisitBoundWhileStatement(BoundWhileStatement whileStatement)
    {
        writer.WriteKeyword(SyntaxKind.WhileKeyword.GetText());
        writer.WriteSpace();
        Visit(whileStatement.Condition);
        writer.WriteLine();
        WriteNestedStatement(whileStatement.Body);
    }
    protected override void VisitBoundNopStatement(BoundNopStatement nopStatement)
    {
        writer.WritePunctuation("nop");
    }
    protected override void VisitBoundSequencePointStatement(BoundSequencePointStatement sequencePointStatement)
    {
        writer.WritePunctuation("seq");
        base.VisitBoundSequencePointStatement(sequencePointStatement);
    }

    void WriteNestedStatement(BoundStatement statement)
    {
        if (statement is BoundBlockStatement block)
        {
            writer.WritePunctuation(SyntaxKind.OpenBraceToken.GetText());
            writer.WriteLine();
            writer.Indent++;
            foreach (var child in block.Statements)
            {
                Visit(child);
                writer.WriteLine();
            }
            writer.Indent--;
            writer.WritePunctuation(SyntaxKind.ClosedBraceToken.GetText());
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
            writer.WritePunctuation(SyntaxKind.OpenParenthesisToken.GetText());

        Visit(expression);

        if (parenthesis)
            writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken.GetText());
    }

    static void WriteFunction(IndentedTextWriter writer, FunctionSymbol function, BoundBlockStatement body)
    {
        function.WriteTo(writer);
        writer.WriteLine();
        Print(body, writer);
        writer.WriteLine();
    }

    public static void Print(BoundNode boundNode, TextWriter textWriter)
    {
        IndentedTextWriter? iw = null;
        var indentedTextWriter = textWriter as IndentedTextWriter ?? (iw = new(textWriter, TABSTRING));
        using(iw)
        {
            new BoundTreePrinter(indentedTextWriter).Visit(boundNode);
        }
    }
    public static void Print(BoundProgram boundProgram, TextWriter textWriter)
    {
        IndentedTextWriter? iw = null;
        var indentedTextWriter = textWriter as IndentedTextWriter ?? (iw = new (textWriter, TABSTRING));
        using(iw)
        {
            foreach (var function in boundProgram.Functions)
                WriteFunction(indentedTextWriter, function.Key, function.Value);
            indentedTextWriter.WriteLine();
        }
    }
}
