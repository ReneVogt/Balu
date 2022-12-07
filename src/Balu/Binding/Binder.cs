using System.Linq;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly DiagnosticBag diagnostics = new();

    BoundScope scope;
    BoundExpression? expression;

    Binder(BoundScope? parent) => scope = new(parent);

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
        if (!scope.TryLookup(name, out var variable))
        {
            if (!string.IsNullOrEmpty(name)) diagnostics.ReportUndefinedName(name, node.IdentifierrToken.Span);
            expression = new BoundLiteralExpression(0);
        }
        else
            expression = new BoundVariableExpression(variable);

        return node;
    }
    protected override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        Visit(node.Expression);
        var variable = new VariableSymbol(name, expression!.Type);
        if (!scope.TryDeclare(variable))
            diagnostics.ReportVariableAlreadyDeclared(name, node.IdentifierrToken.Span);
        expression = new BoundAssignmentExpression(variable, expression!);
        return node;
    }

    //public static BoundTree Bind(ExpressionSyntax syntax, VariableDictionary variables)
    //{
    //    var binder = new Binder(variables);
    //    binder.Visit(syntax);
    //    return new(binder.expression!, binder.diagnostics);
    //}
    //public static BoundTree Bind(SyntaxTree syntax, VariableDictionary variables)
    //{
    //    var binder = new Binder(variables);
    //    binder.Visit(syntax.Root);
    //    return new(binder.expression!, syntax.Diagnostics.Concat(binder.diagnostics));
    //}

    public static BoundGlobalScope BindGlobalScope(CompilationUnitSyntax syntax)
    {
        var binder = new Binder(null);
        binder.Visit(syntax);
        return new BoundGlobalScope(null, binder.expression!, binder.scope.GetDeclaredVariables(), binder.diagnostics);
    }

}
