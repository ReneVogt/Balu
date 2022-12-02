using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly List<Diagnostic> diagnostics = new();
    BoundExpression? expression;

    protected override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        expression = new BoundLiteralExpression(node.Value ?? 0);
        return node;
    }
    protected override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node)
    {
        
        Visit(node.Expression);
        var op = BoundUnaryOperator.Bind(node.OperatorToken.Kind, expression!.Type);
        if (op is null)
            diagnostics.Add(Diagnostic.BinderUnaryOperatorTypeMismatch(node.OperatorToken, expression.Type));
        else
            expression = new BoundUnaryExpression(op, expression!);
        return node;
    }
    protected override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        Visit(node.Left);
        var left = expression!;
        Visit(node.Right);
        var right = expression!;

        var op = BoundBinaryOperator.Bind(node.OperatorToken.Kind, left.Type, right.Type);
        if (op is null)
            diagnostics.Add(Diagnostic.BinderBinaryOperatorTypeMismatch(node.OperatorToken, left.Type, right.Type));
        else
            expression = new BoundBinaryExpression(left, op, right);
        return node;
    }

    public static BoundTree Bind(ExpressionSyntax syntax)
    {
        var binder = new Binder();
        binder.Visit(syntax);
        return new(binder.expression!, binder.diagnostics);
    }
    public static BoundTree Bind(SyntaxTree syntax)
    {
        var binder = new Binder();
        binder.diagnostics.AddRange(syntax.Diagnostics);
        binder.Visit(syntax.Root);
        return new(binder.expression!, binder.diagnostics);
    }
}
