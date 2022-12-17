using System.Collections.Generic;
using System.Collections.Immutable;
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
            SetErrorExpression();
        }
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
        {
            if (left.Type != TypeSymbol.Error && right.Type != TypeSymbol.Error)
                diagnostics.ReportBinaryOperatorTypeMismatch(node.OperatorToken, left.Type, right.Type);
            SetErrorExpression();
        }
        else
            boundNode = new BoundBinaryExpression(left, op, right);
        return node;
    }
    protected override SyntaxNode VisitNameExpression(NameExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        if (node.IdentifierrToken.IsMissing)
            SetErrorExpression();
        else if (!scope.TryLookupSymbol(name, out var symbol))
        {
            diagnostics.ReportUndefinedName(node.IdentifierrToken);
            SetErrorExpression();
        }
        else if (symbol.Kind != SymbolKind.Variable)
        {
            diagnostics.ReportSymbolTypeMismatch(node.IdentifierrToken, SymbolKind.Variable, symbol.Kind);
            SetErrorExpression();
        }
        else
            boundNode = new BoundVariableExpression((VariableSymbol)symbol);

        return node;
    }
    protected override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var name = node.IdentifierToken.Text;
        Visit(node.Expression);

        if (!scope.TryLookupSymbol(name, out var symbol))
        {
            diagnostics.ReportUndefinedVariable(node.IdentifierToken);
            SetErrorExpression();
            return node;
        }
        if (symbol.Kind != SymbolKind.Variable)
        {
            diagnostics.ReportSymbolTypeMismatch(node.IdentifierToken, SymbolKind.Variable, symbol.Kind);
            SetErrorExpression();
            return node;
        }

        var variable = (VariableSymbol)symbol;
        if (variable.ReadOnly)
        {
            diagnostics.ReportVariableIsReadOnly(node.IdentifierToken);
            SetErrorExpression();
            return node;
        }

        var expression = (BoundExpression)boundNode!;
        if (!IsError)
        {
            expression = BindConversion(expression, variable.Type);
            if (expression.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.EqualsToken.Span, ((BoundExpression)boundNode!).Type, variable.Type);
        }

        boundNode = new BoundAssignmentExpression(variable, expression);
        return node;
    }
    protected override SyntaxNode VisitCallExpression(CallExpressionSyntax node)
    {
        if (node.Arguments.Count == 1 && LookupType(node.Identifier.Text) is { } castType)
        {
            Visit(node.Arguments[0]);
            if (IsError) return node;
            var argument = (BoundExpression)boundNode!;
            boundNode = BindConversion(argument, castType);
            if (IsError)
            {
                diagnostics.ReportInvalidCast(node.Span, argument.Type, castType);
                return node;
            }
            return node;
        }

        if (!scope.TryLookupSymbol(node.Identifier.Text, out var symbol))
        {
            diagnostics.ReportUndefinedFunction(node.Identifier);
            SetErrorExpression();
            return node;
        }

        if (symbol.Kind != SymbolKind.Function)
        {
            diagnostics.ReportSymbolTypeMismatch(node.Identifier, SymbolKind.Function, symbol.Kind);
            SetErrorExpression();
            return node;
        }

        var function = (FunctionSymbol)symbol;
        if (function.Parameters.Length != node.Arguments.Count)
        {
            diagnostics.ReportWrongNumberOfArguments(node, function);
            SetErrorExpression();
            return node;
        }

        var arguments = ImmutableArray.CreateBuilder<BoundExpression>();
        bool errors = false;
        for(int i=0; i<node.Arguments.Count; i++)
        {
            Visit(node.Arguments[i]);
            if (IsError) return node;
            var argument = (BoundExpression)boundNode!;
            boundNode = BindConversion(argument, function.Parameters[i].Type);
            if (IsError)
            {
                diagnostics.ReportWrongArgumentType(node.Arguments[i].Span, function.Name, function.Parameters[i].Name, function.Parameters[i].Type, argument.Type);
                errors = true;
            }
            arguments.Add((BoundExpression)boundNode!);
        }

        if (errors)
            SetErrorExpression();
        else
            boundNode = new BoundCallExpression(function, arguments.ToImmutable());
        
        return node;
    }
    protected override SyntaxNode VisitBlockStatement(BlockStatementSyntax node)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        scope = new (scope);
        foreach (var statement in node.Statements)
        {
            Visit(statement);
            statements.Add((BoundStatement)boundNode!);
        }

        scope = scope.Parent!;
        boundNode = new BoundBlockStatement(statements.ToImmutable());
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
        if (expression.Type == TypeSymbol.Void)
            diagnostics.ReportExpressionMustHaveValue(node.Expression.Span);
        bool readOnly = node.KeywordToken.Kind == SyntaxKind.LetKeyword;
        var type = BindTypeClause(node.TypeClause) ?? expression.Type;
        var variable = BindVariable(node.IdentifierToken, readOnly, type);
        if (type != expression.Type && expression.Type != TypeSymbol.Error)
        {
            expression = BindConversion(expression, type);
            if (expression.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.EqualsToken.Span, ((BoundExpression)boundNode!).Type, type);
        }
        boundNode = new BoundVariableDeclarationStatement(variable, expression);
        return node;
    }
    protected override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        if (!IsError)
        {
            condition = BindConversion(condition, TypeSymbol.Boolean);
            if (condition.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.Condition.Span, ((BoundExpression)boundNode!).Type, TypeSymbol.Boolean);
        }

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
        if (!IsError)
        {
            condition = BindConversion(condition, TypeSymbol.Boolean);
            if (condition.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.Condition.Span, ((BoundExpression)boundNode!).Type, TypeSymbol.Boolean);
        }

        Visit(node.Body);
        var statement = (BoundStatement)boundNode!;

        boundNode = new BoundWhileStatement(condition, statement);

        return node;
    }
    protected override SyntaxNode VisitDoWhileStatement(DoWhileStatementSyntax node)
    {
        Visit(node.Body);
        var statement = (BoundStatement)boundNode!;
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        if (!IsError)
        {
            condition = BindConversion(condition, TypeSymbol.Boolean);
            if (condition.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.Condition.Span, ((BoundExpression)boundNode!).Type, TypeSymbol.Boolean);
        }

        boundNode = new BoundDoWhileStatement(statement, condition);

        return node;
    }
    protected override SyntaxNode VisitForStatement(ForStatementSyntax node)
    {
        Visit(node.LowerBound);
        var lowerBound = (BoundExpression)boundNode!;
        if (!IsError)
        {
            lowerBound = BindConversion(lowerBound, TypeSymbol.Integer);
            if (lowerBound.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.LowerBound.Span, ((BoundExpression)boundNode!).Type, TypeSymbol.Integer);
        }
        Visit(node.UpperBound);
        var upperBound = (BoundExpression)boundNode!;
        if (!IsError)
        {
            upperBound = BindConversion(upperBound, TypeSymbol.Integer);
            if (upperBound.Type == TypeSymbol.Error)
                diagnostics.ReportCannotConvert(node.UpperBound.Span, ((BoundExpression)boundNode!).Type, TypeSymbol.Integer);
        }

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
        if (!identifier.IsMissing && !scope.TryDeclareSymbol(variable))
            diagnostics.ReportSymbolAlreadyDeclared(identifier);
        return variable;
    }
    TypeSymbol? BindTypeClause(TypeClauseSyntax? syntax)
    {
        if (syntax is null) return null;
        var type = LookupType(syntax.Identifier.Text);
        if (type is null)
            diagnostics.ReportUndefinedType(syntax.Identifier);
        return type;
    }

    static TypeSymbol? LookupType(string name) => name == TypeSymbol.Integer.Name
                                                      ? TypeSymbol.Integer
                                                      : name == TypeSymbol.Boolean.Name
                                                          ? TypeSymbol.Boolean
                                                          : name == TypeSymbol.String.Name
                                                              ? TypeSymbol.String
                                                              : null;
    void SetErrorExpression()
    {
        if (boundNode?.Kind != BoundNodeKind.ErrorExpression) boundNode = new BoundErrorExpression();
    }
    static BoundExpression BindConversion(BoundExpression expression, TypeSymbol targetType)
    {
        if (expression.Type == TypeSymbol.Error) return expression;
        var conversion = Conversion.Classify(expression.Type, targetType);
        if (!conversion.Exists)
            return new BoundErrorExpression();
        if (conversion.IsIdentity || conversion.IsImplicit)
            return expression;
        return new BoundConversionExpression(targetType, expression);
    }

    void BindFunctionDeclarations(IEnumerable<FunctionDeclarationSyntax> declarations)
    {
        foreach( var declaration in declarations) BindFunctionDeclaration(declaration);
    }
    void BindFunctionDeclaration(FunctionDeclarationSyntax declaration)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameterNames = new HashSet<string>();
        foreach (var parameter in declaration.Parameters)
        {
            var name = parameter.Identifier.Text;
            if (!seenParameterNames.Add(name))
                diagnostics.ReportParameterAlreadyDeclared(parameter.Identifier);

            var type = BindTypeClause(parameter.TypeClause) ?? TypeSymbol.Error;
            parameters.Add(new (name, type));
        }

        var returnType = declaration.TypeClause is null ? TypeSymbol.Void : BindTypeClause(declaration.TypeClause) ?? TypeSymbol.Error;
        if (returnType != TypeSymbol.Error && returnType != TypeSymbol.Void)
            diagnostics.ReportLanguageSupportIssue(declaration.TypeClause?.Span ?? declaration.Span, "Functions with non-void return types are not yet supported.");
        var function = new FunctionSymbol(declaration.Identifier.Text, parameters, returnType);
        if (!scope.TryDeclareSymbol(function))
            diagnostics.ReportFunctionAlreadyDeclared(declaration.Identifier);
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax syntax)
    {
        var parentScope = CreateParentScopes(previous);
        var binder = new Binder(parentScope);

        binder.BindFunctionDeclarations(syntax.Members.OfType<FunctionDeclarationSyntax>());
        var statementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
        {
            binder.Visit(globalStatement);
            statementBuilder.Add((BoundStatement)binder.boundNode!);
        }

        var statement = new BoundBlockStatement(statementBuilder.ToImmutable());

        var diagnostics = previous?.Diagnostics.AddRange(binder.diagnostics) ?? binder.diagnostics.ToImmutableArray();
        return new(previous, statement, binder.scope.GetDeclaredSymbols(), diagnostics);
    }
    public static BoundProgram BindProgram(BoundGlobalScope globalScope)
    {
        //var parentScope = CreateParentScopes(globalScope);
        //var functionBodyBuilder = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        //var diagnostics = globalScope.Diagnostics;

        //foreach (var function in globalScope.Symbols.OfType<FunctionSymbol>().Where(function => function.Declaration is not null))
        //{
        //    var functionBinder = new Binder(parentScope);
        //    functionBinder.Visit(function.Declaration!.Body);
        //    var body = (BoundStatement)functionBinder.boundNode!;
        //    var lowered = Lowerer.Lower(body);
        //    functionBodyBuilder.Add(function, lowered);
        //    diagnostics = diagnostics.AddRange(functionBinder.diagnostics);
        //}
        return new(globalScope, ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Empty, globalScope.Diagnostics);
        //return new(globalScope, functionBodyBuilder.ToImmutable(), diagnostics);
    }
    static BoundScope? CreateParentScopes(BoundGlobalScope? previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous is not null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }

        BoundScope parentScope = new (null);
        foreach (var builtInFunction in BuiltInFunctions.GetBuiltInFunctions())
            parentScope.TryDeclareSymbol(builtInFunction);

        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parentScope);
            foreach (var symbol in previous.Symbols)
                scope.TryDeclareSymbol(symbol);
            parentScope = scope;
        }

        return parentScope;
    }
}