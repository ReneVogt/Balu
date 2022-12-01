using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly List<string> diagnostics = new();
    BoundExpression? expression;

    protected override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        expression = new BoundLiteralExpression(node.LiteralToken.Value as int? ?? 0);
        return node;
    }
    protected override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node)
    {
        var operatorKind = node.OperatorToken.Kind switch
        {
            SyntaxKind.PlusToken => BoundUnaryOperatorKind.Identity,
            SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation,
            _ => throw new BindingException($"Cannot bind unary operator kind {node.OperatorToken.Kind}.")
        };
        
        Visit(node.Expression);
        if (expression!.Type != typeof(int))
            diagnostics.Add($"ERROR: Unary operator {operatorKind} cannot be applied to type {expression.Type}.");
        else
            expression = new BoundUnaryExpression(operatorKind, expression!);
        return node;
    }
    protected override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var operatorKind = node.OperatorToken.Kind switch
        {
            SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
            SyntaxKind.MinusToken => BoundBinaryOperatorKind.Substraction,
            SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
            SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
            _ => throw new BindingException($"Cannot bind binary operator kind {node.OperatorToken.Kind}.")
        };
        Visit(node.Left);
        var left = expression!;
        Visit(node.Right);
        var right = expression!;

        if (left.Type != typeof(int) || right.Type != typeof(int))
            diagnostics.Add($"ERROR: Binary operator {operatorKind} cannot be applied to types {left.Type} and {right.Type}.");
        else
            expression = new BoundBinaryExpression(left, operatorKind, right);
        return node;
    }

    public static BoundTree Bind(ExpressionSyntax syntax)
    {
        var binder = new Binder();
        binder.Visit(syntax);
        return new BoundTree(binder.expression!, binder.diagnostics);
    }
    public static BoundTree Bind(SyntaxTree syntax)
    {
        var binder = new Binder();
        binder.diagnostics.AddRange(syntax.Diagnostics);
        binder.Visit(syntax.Root);
        return new BoundTree(binder.expression!, binder.diagnostics);
    }
}
