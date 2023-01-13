using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Lowering;
using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed class Binder : SyntaxTreeVisitor
{
    readonly DiagnosticBag diagnostics = new();
    readonly FunctionSymbol? containingFunction;
    readonly Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> loopStack = new ();
    readonly bool isScript;

    int labelCounter;
    bool isGlobal;
    BoundScope scope;
    BoundNode? boundNode;

    bool IsError => boundNode is BoundExpression { Type: var type } && type == TypeSymbol.Error;

    Binder(bool isScript, BoundScope? parent, FunctionSymbol? containingFunction = null)
    {
        this.isScript = isScript;
        scope = new(parent);
        this.containingFunction = containingFunction;
        if (this.containingFunction is null) return;
        foreach (var parameter in this.containingFunction.Parameters)
            scope.TryDeclareSymbol(parameter);
    }

    public override void Visit(SyntaxNode node)
    {
        isGlobal = false;
        base.Visit(node);
    }
    protected override void VisitGlobalStatement(GlobalStatementSyntax node)
    {
        isGlobal = true;
        base.Visit(node.Statement);
    }
    protected override void VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        boundNode = new BoundLiteralExpression(node, node.Value ?? 0);
    }
    protected override void VisitUnaryExpression(UnaryExpressionSyntax node)
    {
        Visit(node.Expression);
        if (IsError) return;
        var expression = (BoundExpression)boundNode!;
        var op = BoundUnaryOperator.Bind(node.OperatorToken.Kind, expression.Type);
        if (op is null)
        {
            diagnostics.ReportUnaryOperatorTypeMismatch(node.OperatorToken, expression.Type);
            SetErrorExpression(node);
        }
        else
            boundNode = new BoundUnaryExpression(node, op, expression);
    }
    protected override void VisitBinaryExpression(BinaryExpressionSyntax node)
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
            SetErrorExpression(node);
        }
        else
            boundNode = new BoundBinaryExpression(node, left, op, right);
    }
    protected override void VisitNameExpression(NameExpressionSyntax node)
    {
        var name = node.IdentifierrToken.Text;
        if (node.IdentifierrToken.IsMissing)
            SetErrorExpression(node);
        else if (!scope.TryLookupSymbol(name, out var symbol))
        {
            diagnostics.ReportUndefinedName(node.IdentifierrToken);
            SetErrorExpression(node);
        }
        else if (symbol.Kind != SymbolKind.GlobalVariable && symbol.Kind != SymbolKind.LocalVariable && symbol.Kind != SymbolKind.Parameter)
        {
            diagnostics.ReportSymbolNoVariable(node.IdentifierrToken, symbol.Kind);
            SetErrorExpression(node);
        }
        else
            boundNode = new BoundVariableExpression(node, (VariableSymbol)symbol);
    }
    protected override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        var name = node.IdentifierToken.Text;
        Visit(node.Expression);

        if (!scope.TryLookupSymbol(name, out var symbol))
        {
            diagnostics.ReportUndefinedVariable(node.IdentifierToken);
            SetErrorExpression(node.IdentifierToken);
            return;
        }
        if (symbol.Kind != SymbolKind.GlobalVariable && symbol.Kind != SymbolKind.LocalVariable && symbol.Kind != SymbolKind.Parameter)
        {
            diagnostics.ReportSymbolNoVariable(node.IdentifierToken, symbol.Kind);
            SetErrorExpression(node);
            return;
        }

        var variable = (VariableSymbol)symbol;
        if (variable.ReadOnly)
        {
            diagnostics.ReportVariableIsReadOnly(node.IdentifierToken);
            SetErrorExpression(node.IdentifierToken);
            return;
        }

        var expression = (BoundExpression)boundNode!;
        expression = BindConversion(node.EqualsToken, expression, variable.Type);

        boundNode = new BoundAssignmentExpression(node, variable, expression);
    }
    protected override void VisitCallExpression(CallExpressionSyntax node)
    {
        if (node.Arguments.Count == 1 && LookupType(node.Identifier.Text) is { } castType)
        {
            Visit(node.Arguments[0]);
            if (IsError) return;
            var argument = (BoundExpression)boundNode!;
            boundNode = BindConversion(node.Arguments[0], argument, castType, true);
            return;
        }

        if (!scope.TryLookupSymbol(node.Identifier.Text, out var symbol))
        {
            diagnostics.ReportUndefinedFunction(node.Identifier);
            SetErrorExpression(node);
            return;
        }

        if (symbol.Kind != SymbolKind.Function)
        {
            diagnostics.ReportSymbolNoFunction(node.Identifier, symbol.Kind);
            SetErrorExpression(node);
            return;
        }

        var function = (FunctionSymbol)symbol;
        if (function.Parameters.Length != node.Arguments.Count)
        {
            diagnostics.ReportWrongNumberOfArguments(node, function);
            SetErrorExpression(node);
            return;
        }

        var arguments = ImmutableArray.CreateBuilder<BoundExpression>();
        bool errors = false;
        for(int i=0; i<node.Arguments.Count; i++)
        {
            Visit(node.Arguments[i]);
            if (IsError) return;
            var argument = (BoundExpression)boundNode!;
            boundNode = BindConversion(node.Arguments[i], argument, function.Parameters[i].Type);
            if (IsError)
            {
                diagnostics.ReportWrongArgumentType(node.Arguments[i].Location, function.Name, function.Parameters[i].Name, function.Parameters[i].Type, argument.Type);
                errors = true;
            }
            arguments.Add((BoundExpression)boundNode!);
        }

        if (errors)
            SetErrorExpression(node);
        else
            boundNode = new BoundCallExpression(node, function, arguments.ToImmutable());
    }
    protected override void VisitBlockStatement(BlockStatementSyntax node)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        scope = new (scope);
        foreach (var statement in node.Statements)
        {
            Visit(statement);
            statements.Add((BoundStatement)boundNode!);
        }

        scope = scope.Parent!;
        boundNode = new BoundBlockStatement(node, statements.ToImmutable());
    }
    protected override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        bool global = isGlobal;
        Visit(node.Expression);
        var expression = (BoundExpression)boundNode!;
        boundNode = new BoundExpressionStatement(node, expression);
        if (!(isScript && global) &&  
            expression.Kind != BoundNodeKind.ErrorExpression &&
            expression.Kind != BoundNodeKind.AssignmentExpression &&
            expression.Kind != BoundNodeKind.CallExpression)
            diagnostics.ReportInvalidExpressionStatement(node.Location);
    }
    protected override void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
    {
        Visit(node.Expression);
        var expression = (BoundExpression)boundNode!;
        if (expression.Type == TypeSymbol.Void)
            diagnostics.ReportExpressionMustHaveValue(node.Expression.Location);
        bool readOnly = node.KeywordToken.Kind == SyntaxKind.LetKeyword;
        var type = BindTypeClause(node.TypeClause) ?? expression.Type;
        var variable = BindVariable(node.IdentifierToken, readOnly, type, expression.Constant);
        expression = BindConversion(node.EqualsToken, expression, type);
        boundNode = new BoundVariableDeclarationStatement(node, variable, expression);
    }
    protected override void VisitIfStatement(IfStatementSyntax node)
    {
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        condition = BindConversion(node.Condition, condition, TypeSymbol.Boolean);

        Visit(node.ThenStatement);
        var thenStatement = (BoundStatement)boundNode!;

        BoundStatement? elseStatement = null;
        if (node.ElseClause is { Statement: var elseNode })
        {
            Visit(elseNode);
            elseStatement = (BoundStatement)boundNode!;
        }

        boundNode = new BoundIfStatement(node, condition, thenStatement, elseStatement);
    }
    protected override void VisitWhileStatement(WhileStatementSyntax node)
    {
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        condition = BindConversion(node.Condition, condition, TypeSymbol.Boolean);

        var statement = BindLoopStatement(node.Body, out var breakLabel, out var continueLabel);

        boundNode = new BoundWhileStatement(node, condition, statement, breakLabel, continueLabel);
    }
    protected override void VisitDoWhileStatement(DoWhileStatementSyntax node)
    {
        var statement = BindLoopStatement(node.Body, out var breakLabel, out var continueLabel);
        Visit(node.Condition);
        var condition = (BoundExpression)boundNode!;
        condition = BindConversion(node.Condition, condition, TypeSymbol.Boolean);

        boundNode = new BoundDoWhileStatement(node, statement, condition, breakLabel, continueLabel);
    }
    protected override void VisitForStatement(ForStatementSyntax node)
    {
        Visit(node.LowerBound);
        var lowerBound = (BoundExpression)boundNode!;
        lowerBound = BindConversion(node.LowerBound, lowerBound, TypeSymbol.Integer);
        Visit(node.UpperBound);
        var upperBound = (BoundExpression)boundNode!;
        upperBound = BindConversion(node.UpperBound, upperBound, TypeSymbol.Integer);

        scope = new (scope);
        var variable = BindVariable(node.IdentifierToken, false, TypeSymbol.Integer);
        var body = BindLoopStatement(node.Body, out var breakLabel, out var continueLabel);
        boundNode = new BoundForStatement(node, variable, lowerBound, upperBound, body, breakLabel, continueLabel);

        scope = scope.Parent!;
    }
    protected override void VisitContinueStatement(ContinueStatementSyntax node)
    {
        if (!loopStack.TryPeek(out var frame))
        {
            diagnostics.ReportInvalidBreakOrContinue(node.ContinueKeyword);
            SetErrorStatement(node);
        }

        boundNode = new BoundGotoStatement(node, frame.continueLabel);
    }
    protected override void VisitBreakStatement(BreakStatementSyntax node)
    {
        if (!loopStack.TryPeek(out var frame))
        {
            diagnostics.ReportInvalidBreakOrContinue(node.BreakKeyword);
            SetErrorStatement(node);
        }

        boundNode = new BoundGotoStatement(node, frame.breakLabel);
    }
    protected override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        BoundExpression? expression = null;
        if (node.Expression is not null)
        {
            Visit(node.Expression);
            expression = (BoundExpression)boundNode!;
        }

        if (containingFunction is null && !isScript && expression is not null)
        {
            diagnostics.ReportMainCannotReturnValue(node);
            SetErrorStatement(node);
            return;
        }

        var returnType = containingFunction?.ReturnType ?? (isScript ? TypeSymbol.Any : TypeSymbol.Void);
        var functionName = containingFunction?.Name ?? (isScript ? "$eval" : "main");
        
        if (expression is null)
        {
            if (returnType != TypeSymbol.Void && returnType != TypeSymbol.Any)
            {
                diagnostics.ReportReturnMissingValue(node.Location, returnType, functionName);
                expression = new BoundErrorExpression(node);
            }
        }
        else
        {
            expression = BindConversion(node.Expression!, expression, returnType);
            if (expression.Type == TypeSymbol.Error)
                diagnostics.ReportReturnTypeMismatch(node.Expression!.Location, returnType, functionName, ((BoundExpression)boundNode!).Type);
        }
        
        boundNode = new BoundReturnStatement(node, expression);
    }

    VariableSymbol BindVariable(SyntaxToken identifier, bool isReadonly, TypeSymbol type, BoundConstant? constant = null)
    {
        var name = identifier.IsMissing ? "?" : identifier.Text;
        VariableSymbol variable = containingFunction is null
                           ? new GlobalVariableSymbol(name, isReadonly, type, constant)
                           : new LocalVariableSymbol(name, isReadonly, type, constant);
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
    BoundStatement BindLoopStatement(StatementSyntax statement, out BoundLabel breakLabel, out BoundLabel continueLabel)
    {
        breakLabel = new ($"break{labelCounter}");
        continueLabel = new ($"continue{labelCounter}");
        labelCounter++;
        loopStack.Push((breakLabel, continueLabel));
        Visit(statement);
        loopStack.Pop();
        return (BoundStatement)boundNode!;
    }

    static TypeSymbol? LookupType(string name) => name == TypeSymbol.Integer.Name
                                                      ? TypeSymbol.Integer
                                                      : name == TypeSymbol.Boolean.Name
                                                          ? TypeSymbol.Boolean
                                                          : name == TypeSymbol.String.Name
                                                              ? TypeSymbol.String
                                                              : name == TypeSymbol.Any.Name
                                                                ? TypeSymbol.Any
                                                                : null;
    void SetErrorExpression(SyntaxNode node)
    {
        if (boundNode?.Kind != BoundNodeKind.ErrorExpression) boundNode = new BoundErrorExpression(node);
    }

    void SetErrorStatement(SyntaxNode node)
    {
        if (boundNode is BoundExpressionStatement { Expression.Kind : BoundNodeKind.ErrorExpression }) return;
        boundNode = new BoundExpressionStatement(node, new BoundErrorExpression(node));
    }
    BoundExpression BindConversion(SyntaxNode node, BoundExpression expression, TypeSymbol targetType, bool allowExplicit = false)
    {
        if (expression.Type == TypeSymbol.Error) return expression;
        var conversion = Conversion.Classify(expression.Type, targetType);
        if (!conversion.Exists)
        {
            diagnostics.ReportCannotConvert(node.Location, expression.Type, targetType);
            return new BoundErrorExpression(node);
        }

        if (conversion.IsIdentity)
            return expression;

        if (conversion.IsImplicit || allowExplicit)
            return new BoundConversionExpression(node, targetType, expression);

        diagnostics.ReportCannotConvertImplicit(node.Location, expression.Type, targetType);
        return new BoundErrorExpression(node);
    }
    void BindFunctionDeclarations(IEnumerable<FunctionDeclarationSyntax> declarations)
    {
        foreach( var declaration in declarations) BindFunctionDeclaration(declaration);
    }
    void BindFunctionDeclaration(FunctionDeclarationSyntax declaration)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameterNames = new HashSet<string>();
        for (int ordinal = 0; ordinal < declaration.Parameters.Count; ordinal++)
        {
            ParameterSyntax parameter = declaration.Parameters[ordinal];
            var name = parameter.Identifier.Text;
            if (!seenParameterNames.Add(name))
                diagnostics.ReportParameterAlreadyDeclared(parameter.Identifier);

            var type = BindTypeClause(parameter.TypeClause) ?? TypeSymbol.Error;
            parameters.Add(new(name, type, ordinal));
        }

        var returnType = declaration.TypeClause is null ? TypeSymbol.Void : BindTypeClause(declaration.TypeClause) ?? TypeSymbol.Error;
        var function = new FunctionSymbol(declaration.Identifier.Text, parameters.ToImmutable(), returnType, declaration);
        if (!scope.TryDeclareSymbol(function))
            diagnostics.ReportFunctionAlreadyDeclared(declaration.Identifier);
    }

    static BoundBlockStatement Refactor(BoundStatement statement, FunctionSymbol? containingFunction) =>
        Lowerer.Lower(statement, containingFunction);

    public static BoundGlobalScope BindGlobalScope(bool isScript, BoundGlobalScope? previous, ImmutableArray<SyntaxTree> syntaxTrees)
    {
        var parentScope = CreateParentScopes(previous);
        var binder = new Binder(isScript, parentScope);

        binder.BindFunctionDeclarations(syntaxTrees.SelectMany(syntaxTree => syntaxTree.Root.Members.OfType<FunctionDeclarationSyntax>()));

        var globalStatements = syntaxTrees.SelectMany(syntaxTree => syntaxTree.Root.Members.OfType<GlobalStatementSyntax>()).ToArray();
        var statementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var globalStatement in globalStatements)
        {
            binder.Visit(globalStatement);
            statementBuilder.Add((BoundStatement)binder.boundNode!);
        }

        var symbols = binder.scope.GetDeclaredSymbols();

        var treesWithGlobalStatements = syntaxTrees
                                        .Where(syntaxTree => syntaxTree.Root.Members.Any(syntax => syntax.Kind == SyntaxKind.GlobalStatement))
                                        .ToImmutableArray();

        FunctionSymbol entryPoint;
        if (isScript)
            entryPoint = new ("$eval", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Any);
        else
        {
            var main = symbols.OfType<FunctionSymbol>().FirstOrDefault(symbol => symbol.Name == "main");
            var firstGlobalStatements = treesWithGlobalStatements
                                        .Select(syntaxTree => syntaxTree.Root.Members.OfType<GlobalStatementSyntax>().First())
                                        .ToImmutableArray();
            if (main is not null)
            {
                if (main.ReturnType != TypeSymbol.Void || main.Parameters.Any())
                    binder.diagnostics.ReportInvalidMainSignature(main.Declaration!.Identifier.Location);
                if (firstGlobalStatements.Any())
                {
                    binder.diagnostics.ReportCannotMixMainAndGlobalStatements(main.Declaration!.Identifier.Location);
                    foreach (var globalStatement in firstGlobalStatements)
                        binder.diagnostics.ReportCannotMixMainAndGlobalStatements(globalStatement.Location);
                }

                entryPoint = main;
            }
            else
            {
                if (firstGlobalStatements.Length > 1)
                {
                    foreach (var globalStatement in firstGlobalStatements)
                        binder.diagnostics.ReportOnlyOneFileCanHaveGlobalStatements(globalStatement.Location);
                }

                entryPoint = new ("main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
                if (globalStatements.Length == 0)
                    binder.diagnostics.ReportNoEntryPointDefined();
            }
        }

        var syntaxNode = (treesWithGlobalStatements.FirstOrDefault() ?? syntaxTrees.First()).Root;
        var statement = Refactor(new BoundBlockStatement(syntaxNode, statementBuilder.ToImmutable()), null);

        var diagnostics = syntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics).Concat(binder.diagnostics).ToImmutableArray();
        return new(previous, entryPoint, statement, symbols, diagnostics);
    }
    public static BoundProgram BindProgram(bool isScript, BoundProgram? previous, BoundGlobalScope globalScope)
    {
        var functionBodyBuilder = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = globalScope.Diagnostics;
        var parentScope = CreateParentScopes(globalScope);

        foreach (var function in globalScope.Symbols.OfType<FunctionSymbol>().Where(function => function.Declaration is not null))
        {
            var functionBinder = new Binder(isScript, parentScope, function);
            functionBinder.Visit(function.Declaration!.Body);
            var body = (BoundStatement)functionBinder.boundNode!;
            var refactoredBody = Refactor(body, function);
            if (function.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(refactoredBody))
                functionBinder.diagnostics.ReportNotAllPathsReturn(function);
            functionBodyBuilder.Add(function, refactoredBody);
            diagnostics = diagnostics.AddRange(functionBinder.diagnostics);
        }

        var refactoredEntryPoint = Refactor(globalScope.Statement, globalScope.EntryPoint);
        functionBodyBuilder.Add(globalScope.EntryPoint, refactoredEntryPoint);

        return new(previous, globalScope.EntryPoint, globalScope.Symbols, functionBodyBuilder.ToImmutable(), diagnostics);
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