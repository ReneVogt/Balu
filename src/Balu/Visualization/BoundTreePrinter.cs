using System.IO;
using Balu.Binding;
using Balu.Syntax;

namespace Balu.Visualization;

sealed class BoundTreePrinter : BoundExpressionVisitor
{
    readonly TextWriter writer;
    string indent = string.Empty;
    bool last = true;

    BoundTreePrinter(TextWriter writer) => this.writer = writer;

    /// <inheritdoc />
    public override BoundExpression Visit(BoundExpression expression)
    {
        var marker = last ? TreeTexts.LastLeaf : TreeTexts.Leaf;
        writer.Write(indent);
        writer.Write(marker);

        base.Visit(expression);
        return expression;
    }
    protected override BoundExpression VisitBoundLiteralExpression(BoundLiteralExpression literalExpression)
    {
        writer.WriteLine($"{literalExpression.Kind}({literalExpression.Type}) {literalExpression.Value}");
        return literalExpression;
    }
    protected override BoundExpression VisitBoundUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        writer.WriteLine($"{unaryExpression.OperatorKind}({unaryExpression.Type})");
        var lastIndnet = indent;
        var lastLast = last;
        indent += last ? TreeTexts.Indent : TreeTexts.Branch;
        last = true;
        Visit(unaryExpression.Operand);
        last = lastLast;
        indent = lastIndnet;
        return unaryExpression;
    }
    protected override BoundExpression VisitBoundBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        writer.WriteLine($"{binaryExpression.OperatorKind}({binaryExpression.Type})");
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
    public static void Print(BoundExpression boundExpression, TextWriter textWriter) => new BoundTreePrinter(textWriter).Visit(boundExpression);
}
