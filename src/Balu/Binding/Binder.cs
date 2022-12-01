using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly List<string> diagnostics = new();
    BoundExpression? expression;

    protected override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        expression = new BoundLiteralExpression(node.Value ?? 0);
        return node;
    }
    protected override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node)
    {
        var operatorKind = node.OperatorToken.Kind.UnaryOperatorKind(); 
        
        Visit(node.Expression);
        if (!operatorKind.CanBeAppliedTo(expression!.Type))
            diagnostics.Add($"ERROR: Unary operator {operatorKind} cannot be applied to type {expression.Type}.");
        else
            expression = new BoundUnaryExpression(operatorKind, expression!);
        return node;
    }
    protected override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var operatorKind = node.OperatorToken.Kind.BinaryOperatorKind(); 
        Visit(node.Left);
        var left = expression!;
        Visit(node.Right);
        var right = expression!;

        if (!operatorKind.CanBeAppliedTo(left.Type, right.Type))
            diagnostics.Add($"ERROR: Binary operator {operatorKind} cannot be applied to types {left.Type} and {right.Type}.");
        else
            expression = new BoundBinaryExpression(left, operatorKind, right);
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
