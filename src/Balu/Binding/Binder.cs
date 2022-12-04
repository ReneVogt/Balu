using System;
using System.Collections.Generic;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly List<Diagnostic> diagnostics = new();
    readonly Dictionary<string, object?> variables;
    BoundExpression? expression;

    Binder(Dictionary<string, object?> variables) => this.variables = variables;

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
    protected override SyntaxNode VisitNameExpression(NameExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        if (variables.TryGetValue(name, out var value))
            expression = new BoundVariableExpression(name, value?.GetType() ?? typeof(object));
        else
        {
            diagnostics.Add(Diagnostic.BinderUndefinedName(name, node.IdentifierrToken.TextSpan));
            expression = new BoundLiteralExpression(0);
        }
        return node;
    }
    protected override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        throw new NotImplementedException();
    }

    public static BoundTree Bind(ExpressionSyntax syntax, Dictionary<string, object?> variables)
    {
        var binder = new Binder(variables);
        binder.Visit(syntax);
        return new(binder.expression!, binder.diagnostics);
    }
    public static BoundTree Bind(SyntaxTree syntax, Dictionary<string, object?> variables)
    {
        var binder = new Binder(variables);
        binder.diagnostics.AddRange(syntax.Diagnostics);
        binder.Visit(syntax.Root);
        return new(binder.expression!, binder.diagnostics);
    }
}
