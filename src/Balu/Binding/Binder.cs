using System.Linq;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly DiagnosticBag diagnostics = new();
    readonly VariableDictionary variables;
    BoundExpression? expression;

    Binder(VariableDictionary variables) => this.variables = variables;

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
            diagnostics.ReportUnaryOperatorTypeMismatch(node.OperatorToken, expression.Type);
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
            diagnostics.ReportBinaryOperatorTypeMismatch(node.OperatorToken, left.Type, right.Type);
        else
            expression = new BoundBinaryExpression(left, op, right);
        return node;
    }
    protected override SyntaxNode VisitNameExpression(NameExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        var symbol = variables.Keys.FirstOrDefault(s => s.Name == name);
        if (symbol is null)
        {
            diagnostics.ReportUndefinedName(name, node.IdentifierrToken.Span);
            expression = new BoundLiteralExpression(0);
        }
        else
            expression = new BoundVariableExpression(symbol);

        return node;
    }
    protected override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        Visit(node.Expression);

        var symbol = variables.Keys.FirstOrDefault(s => s.Name == name);
        if (symbol is not null) variables.Remove(symbol);
        symbol = new (name, expression!.Type);
        variables[symbol] = null;
        expression = new BoundAssignmentExpression(symbol, expression!);
        return node;
    }

    public static BoundTree Bind(ExpressionSyntax syntax, VariableDictionary variables)
    {
        var binder = new Binder(variables);
        binder.Visit(syntax);
        return new(binder.expression!, binder.diagnostics);
    }
    public static BoundTree Bind(SyntaxTree syntax, VariableDictionary variables)
    {
        var binder = new Binder(variables);
        binder.Visit(syntax.Root);
        return new(binder.expression!, syntax.Diagnostics.Concat(binder.diagnostics));
    }
}
