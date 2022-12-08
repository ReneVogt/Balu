using System;
using System.IO;
using Balu.Binding;
using Balu.Syntax;

namespace Balu.Visualization;

sealed class BoundTreeWriter : BoundTreeVisitor
{
    readonly TextWriter writer;
    readonly bool console;
    string indent = string.Empty;
    bool last = true;

    BoundTreeWriter(TextWriter writer) => (this.writer, console) = (writer, writer == Console.Out);

    /// <inheritdoc />
    public override BoundNode Visit(BoundNode node)
    {
        var marker = last ? TreeTexts.LastLeaf : TreeTexts.Leaf;
        writer.Write(indent);
        if (console) Console.ForegroundColor = ConsoleColor.DarkGray;
        writer.Write(marker);
        if (console) Console.ResetColor();

        base.Visit(node);
        return node;
    }
    protected override BoundNode VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        if (console) Console.ForegroundColor = ConsoleColor.DarkYellow;
        writer.WriteLine($"{literalExpression.Kind}({literalExpression.Type}) {literalExpression.Value}");
        if (console) Console.ResetColor();
        return literalExpression;
    }
    protected override BoundNode VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        if (console) Console.ForegroundColor = ConsoleColor.DarkYellow;
        writer.WriteLine($"{unaryExpression.Operator.OperatorKind}({unaryExpression.Type})");
        if (console) Console.ResetColor();
        var lastIndnet = indent;
        var lastLast = last;
        indent += last ? TreeTexts.Indent : TreeTexts.Branch;
        last = true;
        Visit(unaryExpression.Operand);
        last = lastLast;
        indent = lastIndnet;
        return unaryExpression;
    }
    protected override BoundNode VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        if (console) Console.ForegroundColor = ConsoleColor.DarkYellow;
        writer.WriteLine($"{binaryExpression.Operator.OperatorKind}({binaryExpression.Type})");
        if (console) Console.ResetColor();
        var lastIndnet = indent;
        var lastLast = last;
        indent += last ? TreeTexts.Indent : TreeTexts.Branch;
        last = false;
        Visit(binaryExpression.Left);
        last = true;
        Visit(binaryExpression.Right);
        last = lastLast;
        indent = lastIndnet;
        return binaryExpression;
    }

    /// <summary>
    /// Writes a text-based tree representation of <paramref name="boundExpression"/> to <paramref name="textWriter"/>.
    /// </summary>
    /// <param name="boundExpression">The <see cref="ExpressionSyntax"/> to represent.</param>
    /// <param name="textWriter">The <see cref="TextWriter"/> to write the output to.</param>
    public static void Print(BoundNode boundNode, TextWriter textWriter) => new BoundTreeWriter(textWriter).Visit(boundNode);
}
