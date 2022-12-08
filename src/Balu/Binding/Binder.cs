using System.Collections.Generic;
using System.Linq;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly DiagnosticBag diagnostics = new();

    BoundScope scope;
    BoundNode? boundNode;

    Binder(BoundScope? parent) => scope = new(parent);

    protected override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        boundNode = new BoundLiteralExpression(node.Value ?? 0);
        return node;
    }
    protected override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node)
    {
        
        Visit(node.Expression);
        var expression = (BoundExpression)boundNode!;
        var op = BoundUnaryOperator.Bind(node.OperatorToken.Kind, expression.Type);
        if (op is null)
            diagnostics.ReportUnaryOperatorTypeMismatch(node.OperatorToken, expression.Type);
        else
            boundNode = new BoundUnaryExpression(op, expression);
        return node;
    }
    protected override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        Visit(node.Left);
        var left = (BoundExpression)boundNode!;
        Visit(node.Right);
        var right = (BoundExpression)boundNode!;

        var op = BoundBinaryOperator.Bind(node.OperatorToken.Kind, left.Type, right.Type);
        if (op is null)
            diagnostics.ReportBinaryOperatorTypeMismatch(node.OperatorToken, left.Type, right.Type);
        else
            boundNode = new BoundBinaryExpression(left, op, right);
        return node;
    }
    protected override SyntaxNode VisitNameExpression(NameExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        if (!scope.TryLookup(name, out var variable))
        {
            if (!string.IsNullOrEmpty(name)) diagnostics.ReportUndefinedName(node.IdentifierrToken);
            boundNode = new BoundLiteralExpression(0);
        }
        else
            boundNode = new BoundVariableExpression(variable);

        return node;
    }
    protected override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        Visit(node.Expression);

        var expression = (BoundExpression)boundNode!;

        if (!scope.TryLookup(name, out var variable))
            diagnostics.ReportUndefinedName(node.IdentifierrToken);
        else if (variable.ReadOnly)
            diagnostics.ReportVariableIsReadOnly(node.IdentifierrToken);
        else if (expression.Type != variable.Type)
            diagnostics.ReportCannotConvert(node.EqualsToken.Span, expression.Type, variable.Type);
        else
            boundNode = new BoundAssignmentExpression(variable, expression);
        return node;
    }
    protected override SyntaxNode VisitBlockStatement(BlockStatementSyntax node)
    {
        var statements = new List<BoundStatement>();
        scope = new (scope);
        foreach (var statement in node.Statements)
        {
            Visit(statement);
            statements.Add((BoundStatement)boundNode!);
        }

        scope = scope.Parent!;
        boundNode = new BoundBlockStatement(statements);
        return node;
    }
    protected override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        Visit(node.Expression);
        boundNode = new BoundExpressionStatement((BoundExpression)boundNode!);
        return node;
    }
    protected override SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationSyntax node)
    {
        Visit(node.Expression);
        var expression = (BoundExpression)boundNode!;
        string name = node.IdentifierToken.Text;
        bool readOnly = node.KeywordToken.Kind == SyntaxKind.LetKeyword;

        var variable = new VariableSymbol(name, readOnly, expression.Type);
        if (!scope.TryDeclare(variable))
            diagnostics.ReportVariableAlreadyDeclared(node.IdentifierToken);
        boundNode = new BoundVariableDeclaration(variable, expression);
        return node;
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax syntax)
    {
        var binder = new Binder(CreateParentScopes(previous));
        binder.Visit(syntax);
        var diagnostics = previous is null ? binder.diagnostics : previous.Diagnostics.Concat(binder.diagnostics);
        return new (previous, (BoundStatement)binder.boundNode!, binder.scope.GetDeclaredVariables(), diagnostics);
    }
    static BoundScope? CreateParentScopes(BoundGlobalScope? previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous is not null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }

        BoundScope? parentScope = null;
        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parentScope);
            foreach (var variable in previous.Variables)
                scope.TryDeclare(variable);
            parentScope = scope;
        }

        return parentScope;
    }
}
