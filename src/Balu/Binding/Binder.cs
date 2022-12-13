using System.Collections.Generic;
using System.Linq;
using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxVisitor
{
    readonly DiagnosticBag diagnostics = new();

    BoundScope scope;
    BoundNode? boundNode;

    bool IsError => boundNode is BoundExpression { Type: var type } && type == TypeSymbol.Error;

    Binder(BoundScope? parent) => scope = new(parent);

    protected override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        boundNode = new BoundLiteralExpression(node.Value ?? 0);
        return node;
    }
    protected override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node)
    {
        Visit(node.Expression);
        if (IsError) return node;
        var expression = (BoundExpression)boundNode!;
        var op = BoundUnaryOperator.Bind(node.OperatorToken.Kind, expression.Type);
        if (op is null)
        {
            diagnostics.ReportUnaryOperatorTypeMismatch(node.OperatorToken, expression.Type);
            boundNode = new BoundErrorExpression();
        }
        else
            boundNode = new BoundUnaryExpression(op, expression);
        return node;
    }
    protected override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        Visit(node.Left);
        var left = (BoundExpression)boundNode!;
        bool error = IsError;
        Visit(node.Right);
        if (error || IsError) return node;
        var right = (BoundExpression)boundNode!;

        var op = BoundBinaryOperator.Bind(node.OperatorToken.Kind, left.Type, right.Type);
        if (op is null)
        {
            diagnostics.ReportBinaryOperatorTypeMismatch(node.OperatorToken, left.Type, right.Type);
            boundNode = new BoundErrorExpression();
        }
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
            boundNode = new BoundErrorExpression();
        }
        else
            boundNode = new BoundVariableExpression(variable);

        return node;
    }
    protected override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var name = node.IdentifierToken.Text;
        Visit(node.Expression);
        if (IsError) return node;

        var expression = (BoundExpression)boundNode!;

        if (!scope.TryLookup(name, out var variable))
            diagnostics.ReportUndefinedName(node.IdentifierToken);
        else if (variable.ReadOnly)
            diagnostics.ReportVariableIsReadOnly(node.IdentifierToken);
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
    protected override SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
    {
        Visit(node.Expression);
        var expression = (BoundExpression)boundNode!;
        bool readOnly = node.KeywordToken.Kind == SyntaxKind.LetKeyword;
        var variable = BindVariable(node.IdentifierToken, readOnly, expression.Type);
        boundNode = new BoundVariableDeclarationStatement(variable, expression);
        return node;
    }
    protected override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        if (condition.Type != TypeSymbol.Boolean)
            diagnostics.ReportCannotConvert(node.Condition.Span, condition.Type, TypeSymbol.Boolean);

        Visit(node.ThenStatement);
        var thenStatement = (BoundStatement)boundNode!;

        BoundStatement? elseStatement = null;
        if (node.ElseClause is { Statement: var elseNode })
        {
            Visit(elseNode);
            elseStatement = (BoundStatement)boundNode!;
        }

        boundNode = new BoundIfStatement(condition, thenStatement, elseStatement);

        return node;
    }
    protected override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
    {
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        if (condition.Type != TypeSymbol.Boolean)
            diagnostics.ReportCannotConvert(node.Condition.Span, condition.Type, TypeSymbol.Boolean);

        Visit(node.Body);
        var statement = (BoundStatement)boundNode!;

        boundNode = new BoundWhileStatement(condition, statement);

        return node;
    }
    protected override SyntaxNode VisitForStatement(ForStatementSyntax node)
    {
        Visit(node.LowerBound);
        var lowerBound = (BoundExpression)boundNode!;
        if (lowerBound.Type != TypeSymbol.Integer)
            diagnostics.ReportCannotConvert(node.LowerBound.Span, lowerBound.Type, TypeSymbol.Integer);
        Visit(node.UpperBound);
        var upperBound = (BoundExpression)boundNode!;
        if (upperBound.Type != TypeSymbol.Integer)
            diagnostics.ReportCannotConvert(node.UpperBound.Span, upperBound.Type, TypeSymbol.Integer);

        scope = new (scope);
        var variable = BindVariable(node.IdentifierToken, true, TypeSymbol.Integer);
        Visit(node.Body);
        var body = (BoundStatement)boundNode!;
        boundNode = new BoundForStatement(variable, lowerBound, upperBound, body);

        scope = scope.Parent!;

        return node;
    }

    VariableSymbol BindVariable(SyntaxToken identifier, bool isReadonly, TypeSymbol type)
    {
        var name = identifier.IsMissing ? "?" : identifier.Text;
        var variable = new VariableSymbol(name, isReadonly, type);
        if (!identifier.IsMissing && !scope.TryDeclare(variable))
            diagnostics.ReportVariableAlreadyDeclared(identifier);
        return variable;
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
