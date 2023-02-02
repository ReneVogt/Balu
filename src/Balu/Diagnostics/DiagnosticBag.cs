using System;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Balu.Diagnostics;

sealed class DiagnosticBag : List<Diagnostic>
{
    public DiagnosticBag() { }
    public DiagnosticBag(IEnumerable<Diagnostic> diagnostics) : base(diagnostics) { }

    public void ReportUnexpectedToken(TextLocation location) =>
        Add(new(DiagnosticId.UnexpectedToken, location, $"Unexpected token '{location.Text.ToString(location.Span)}'."));
    public void ReportNumberNotValid(TextLocation location, string text) =>
        Add(new(DiagnosticId.NumberNotValid, location, $"The number '{text}' is not a valid 32bit integer."));
    public void ReportInvalidEscapeSequence(TextLocation location, string text) =>
        Add(new(DiagnosticId.InvalidEscapeSequence, location, $"Invalid escape sequence '{text}'."));
    public void ReportUnterminatedString(TextLocation location) =>
        Add(new(DiagnosticId.UnterminatedString, location, "String literal not terminated."));
    public void ReportUnexpectedToken(SyntaxToken foundToken, SyntaxKind expected) =>
        Add(new(DiagnosticId.UnexpectedToken, foundToken.Location, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}."));
    public void ReportUnterminatedMultiLineComment(TextLocation location) =>
        Add(new(DiagnosticId.UnterminatedMultilineComment, location, "Unterminated multiline comment."));

    public void ReportUnaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol type) =>
        Add(new(DiagnosticId.UnaryOperatorTypeMismtach, operatorToken.Location, $"Unary operator '{operatorToken.Text}' cannot be applied to type '{type.Name}'."));
    public void ReportBinaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol left, TypeSymbol right) => Add(
        new(DiagnosticId.BinaryOperatorTypeMismatch, operatorToken.Location, $"Binary operator '{operatorToken.Text}' cannot be applied to types '{left.Name}' and '{right.Name}'."));
    public void ReportPostfixExpressionTypeMismatch(PostfixExpressionSyntax syntax, TypeSymbol variableType) => Add(
        new(DiagnosticId.PostfixExpressionTypeMismatch, syntax.Location, $"Postfix operator '{syntax.OperatorToken.Text}' cannot be applied to type '{variableType.Name}'."));
    public void ReportPrefixExpressionTypeMismatch(PrefixExpressionSyntax syntax, TypeSymbol variableType) => Add(
        new(DiagnosticId.PrefixExpressionTypeMismatch, syntax.Location, $"Prefix operator '{syntax.OperatorToken.Text}' cannot be applied to type '{variableType.Name}'."));
    public void ReportUndefinedName(SyntaxToken identifierToken) => Add(new(DiagnosticId.UndefinedName, identifierToken.Location, $"Undefined name '{identifierToken.Text}'."));
    public void ReportCannotConvert(TextLocation location, TypeSymbol sourceType, TypeSymbol targetType) => Add(new(DiagnosticId.CannotConvert, location, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'."));
    public void ReportCannotConvertImplicit(TextLocation location, TypeSymbol sourceType, TypeSymbol targetType) => Add(new(DiagnosticId.CannotConvertImplicit, location, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'. An explicit conversion exists, are you missing a cast?"));
    public void ReportSymbolAlreadyDeclared(SyntaxToken identifierToken) => Add(new(DiagnosticId.SymbolAlreadyDeclared, identifierToken.Location, $"Symbol '{identifierToken.Text}' is already declared."));
    public void ReportSymbolHidesSymbol(Symbol hiding, Symbol hidden, TextLocation location)
    {
        var hidingKind = hiding.Kind switch
        {
            SymbolKind.LocalVariable => "Local variable ",
            SymbolKind.Parameter => "Parameter ",
            SymbolKind.Function => "Function ",
            SymbolKind.GlobalVariable => "Global variable ",
            _ => string.Empty
        };
        var hiddenKind = hidden.Kind switch
        {
            SymbolKind.LocalVariable => "outer scope variable ",
            SymbolKind.Parameter => "parameter ",
            SymbolKind.Function => "existing function ",
            SymbolKind.GlobalVariable => "global variable ",
            _ => string.Empty
        };
        Add(new(DiagnosticId.SymbolHidesSymbol, location, $"{hidingKind}'{hiding.Name}' hides {hiddenKind}'{hidden.Name}'.", severity: DiagnosticSeverity.Warning));
    }
    public void ReportVariableIsReadOnly(SyntaxToken identifierToken) => Add(new(DiagnosticId.VariableIsReadOnly, identifierToken.Location, $"Variable '{identifierToken.Text}' is readonly and cannot be assigned to."));
    public void ReportWrongNumberOfArguments(CallExpressionSyntax syntax, FunctionSymbol function)
    {
        var location = syntax.Arguments.Count < function.Parameters.Length
                           ? syntax.ClosedParenthesis.Location
                           : new(syntax.SyntaxTree.Text,
                                 new(
                                     syntax.Arguments.ElementsWithSeparators[Math.Max(0, function.Parameters.Length * 2 - 1)].Span.Start,
                                     syntax.Arguments.ElementsWithSeparators.Skip(function.Parameters.Length * 2 - 1)
                                           .Aggregate(0, (acc, token) => acc + token.Span.Length)));
        Add(
            new(DiagnosticId.WrongNumberOfArguments,
                location,
                $"Function '{syntax.Identifier.Text}' takes {function.Parameters.Length} parameters, but is invoked with {syntax.Arguments.Count} arguments."));
    }
    public void ReportWrongArgumentType(TextLocation location, string functionName, string parameterName, TypeSymbol expectedType, TypeSymbol actualType) => Add(
        new(DiagnosticId.WrongNumberOfArguments, location,
            $"Parameter '{parameterName}' of function '{functionName}' requires a value of type '{expectedType}', but was given a value of type '{actualType}'."));
    public void ReportExpressionMustHaveValue(TextLocation location) => Add(new(DiagnosticId.ExpressionMustHaveValue, location, "Expression must return a value."));
    public void ReportSymbolNoVariable(SyntaxToken identifier, SymbolKind actual) => Add(new(DiagnosticId.SymbolNoVariable, identifier.Location, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a variable or argument."));
    public void ReportSymbolNoFunction(SyntaxToken identifier, SymbolKind actual) => Add(new(DiagnosticId.SymbolNoFunction, identifier.Location, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a function."));
    public void ReportUndefinedType(SyntaxToken identifier) => Add(new(DiagnosticId.UndefinedType, identifier.Location, $"Undefined type '{identifier.Text}'."));
    public void ReportUndefinedVariable(SyntaxToken identifier) => Add(new(DiagnosticId.UndefinedVariable, identifier.Location, $"Undefined variable '{identifier.Text}'."));
    public void ReportUndefinedFunction(SyntaxToken identifier) => Add(new(DiagnosticId.UndefinedFunction, identifier.Location, $"Undefined function '{identifier.Text}'."));
    public void ReportParameterAlreadyDeclared(SyntaxToken identifier) => Add(new(DiagnosticId.ParameterAlreadyDeclared, identifier.Location, $"Parameter '{identifier.Text}' is already declared."));
    public void ReportFunctionAlreadyDeclared(SyntaxToken identifier) => Add(new(DiagnosticId.FunctionAlreadyDeclared, identifier.Location, $"Function '{identifier.Text}' is already declared."));
    public void ReportInvalidBreakOrContinue(SyntaxToken keyword) => Add(new(DiagnosticId.InvalidBreakOrContinue, keyword.Location, $"Invalid '{keyword.Text}' outside any loop."));
    public void ReportMainCannotReturnValue(ReturnStatementSyntax returnStatement) => Add(new(DiagnosticId.MainCannotReturnValue, returnStatement.Location, $"'{GlobalSymbolNames.Main}' cannot return a value."));
    public void ReportReturnMissingValue(TextLocation location, TypeSymbol returnType, string functionName) => Add(new(DiagnosticId.ReturnMissingValue, location, $"'{functionName}' needs to return a value of type '{returnType}'."));
    public void ReportReturnTypeMismatch(TextLocation location, TypeSymbol returnType, string functionName, TypeSymbol actualType) => Add(new(DiagnosticId.ReturnTypeMismatch, location,
        returnType == TypeSymbol.Void
            ? $"'{functionName}' does not have a return type and cannot return a value of type '{actualType}'."
            : $"'{functionName}' needs to return a value of type '{returnType}', not '{actualType}'."));
    public void ReportNotAllPathsReturn(FunctionSymbol function) => Add(new(DiagnosticId.NotAllPathsReturn, function.Declaration?.Body.ClosedBraceToken.Location ?? default, $"Not all code paths of function '{function.Name}' return a value of type '{function.ReturnType}'."));
    public void ReportInvalidExpressionStatement(TextLocation location) => Add(new(DiagnosticId.InvalidExpressionStatement, location, "Only assignment, call increment or decrement expressions can be used as a statement."));
    public void ReportCannotMixMainAndGlobalStatements(TextLocation location) =>
        Add(new(DiagnosticId.CannotMixMainANdGlobalStatements, location, $"Global statements cannot be mixed with a '{GlobalSymbolNames.Main}' function."));
    public void ReportInvalidMainSignature(TextLocation location) =>
        Add(new(DiagnosticId.InvalidMainSignature, location, $"'{GlobalSymbolNames.Main}' must be parameterless and of type 'void'."));
    public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location) =>
        Add(new(DiagnosticId.OnlyOneFileCanHaveGlobalStatements, location, "At most one file can contain global statements."));
    public void ReportNoEntryPointDefined() =>
        Add(new(DiagnosticId.NoEntryPointDefined, default, $"No entry point found (neither a '{GlobalSymbolNames.Main}' function nor global statements)."));
    public void ReportUnreachableCode(TextLocation location) => Add(new(DiagnosticId.UnreachableCode, location, "Unreachable code detected.", DiagnosticSeverity.Warning));

    public void ReportInvalidAssemblyReference(string reference, string exceptionMessage) =>
        Add(new(DiagnosticId.InvalidAssemblyReference, default, $"Could not load referenced assembly '{reference}': {exceptionMessage}"));
    public void ReportRequiredTypeNotFound(string metaDataname, TypeSymbol? type = null) =>
        Add(new(
                DiagnosticId.RequiredTypeNotFound,
                default,
                type is null
                    ? $"The required type '{metaDataname}' could not be resolved among the referenced assemblies."
                    : $"The required type '{type.Name}' ('{metaDataname}') could not be resolved among the referenced assemblies."));
    public void ReportRequiredTypeAmbiguous(string name, TypeDefinition[] typeDefinitions) =>
        Add(new(DiagnosticId.RequiredTypeAmbiguous, default, $"The required type '{name}' is ambiguous among these referenced assemblies: {string.Join(", ", typeDefinitions.Select(t => t.Module.Assembly.Name.Name).OrderBy(n => n))}."));
    public void ReportRequiredMethodNotFound(string type, string name, string[] parameterTypeNames) =>
        Add(new(DiagnosticId.RequiredMethodNotFound, default, $"The required method'{type}.{name}({string.Join(", ", parameterTypeNames)})' could not be found in the referenced assemblies."));
}
