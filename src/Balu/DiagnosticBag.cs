using System;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Balu;

sealed class DiagnosticBag : List<Diagnostic>
{
    public DiagnosticBag(){}
    public DiagnosticBag(IEnumerable<Diagnostic> diagnostics) : base(diagnostics){}

    public void ReportUnexpectedToken(TextLocation location) =>
        Add(new("BL0001", location, $"Unexpected token '{location.Text.ToString(location.Span)}'."));
    public void ReportNumberNotValid(TextLocation location, string text) =>
        Add(new("BL0002", location, $"The number '{text}' is not a valid 32bit integer."));
    public void ReportInvalidEscapeSequence(TextLocation location, string text) =>
        Add(new("BL0003", location, $"Invalid escape sequence '{text}'."));
    public void ReportUnterminatedString(TextLocation location) =>
        Add(new("BL0004", location, "String literal not terminated."));
    public void ReportUnexpectedToken(SyntaxToken foundToken, SyntaxKind expected) =>
        Add(new("BL0005", foundToken.Location, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}."));
    public void ReportUnterminatedMultiLineComment(TextLocation location) =>
        Add(new("BL0006", location, "Unterminated multiline comment."));

    public void ReportUnaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol type) =>
        Add(new("BL1001", operatorToken.Location, $"Unary operator '{operatorToken.Text}' cannot be applied to type '{type.Name}'."));
    public void ReportBinaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol left, TypeSymbol right) => Add(
        new("BL1002", operatorToken.Location, $"Binary operator '{operatorToken.Text}' cannot be applied to types '{left.Name}' and '{right.Name}'."));
    public void ReportUndefinedName(SyntaxToken identifierToken) => Add(new("BL1003", identifierToken.Location, $"Undefined name '{identifierToken.Text}'."));
    public void ReportCannotConvert(TextLocation location, TypeSymbol sourceType, TypeSymbol targetType) => Add(new("BL1004", location, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'."));
    public void ReportCannotConvertImplicit(TextLocation location, TypeSymbol sourceType, TypeSymbol targetType) => Add(new("BL1005", location, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'. An explicit conversion exists, are you missing a cast?"));
    public void ReportSymbolAlreadyDeclared(SyntaxToken identifierToken) => Add(new("BL1006", identifierToken.Location, $"Symbol '{identifierToken.Text}' is already declared."));
    public void ReportVariableIsReadOnly(SyntaxToken identifierToken) => Add(new("BL1007", identifierToken.Location, $"Variable '{identifierToken.Text}' is readonly and cannot be assigned to."));
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
            new("BL1008",
                location,
                $"Function '{syntax.Identifier.Text}' takes {function.Parameters.Length} parameters, but is invoked with {syntax.Arguments.Count} arguments."));
    }
    public void ReportWrongArgumentType(TextLocation location, string functionName, string parameterName, TypeSymbol expectedType, TypeSymbol actualType) => Add(
        new("BL1009", location,
            $"Parameter '{parameterName}' of function '{functionName}' requires a value of type '{expectedType}', but was given a value of type '{actualType}'."));
    public void ReportExpressionMustHaveValue(TextLocation location) => Add(new("BL1010", location,"Expression must return a value."));
    public void ReportSymbolNoVariable(SyntaxToken identifier, SymbolKind actual) => Add(new("BL1011", identifier.Location, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a variable or argument."));
    public void ReportSymbolNoFunction(SyntaxToken identifier, SymbolKind actual) => Add(new("BL1012", identifier.Location, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a function."));
    public void ReportUndefinedType(SyntaxToken identifier) => Add(new("BL1013", identifier.Location, $"Undefined type '{identifier.Text}'."));
    public void ReportUndefinedVariable(SyntaxToken identifier) => Add(new("BL1014", identifier.Location, $"Undefined variable '{identifier.Text}'."));
    public void ReportUndefinedFunction(SyntaxToken identifier) => Add(new("BL1015", identifier.Location, $"Undefined function '{identifier.Text}'."));
    public void ReportParameterAlreadyDeclared(SyntaxToken identifier) => Add(new("BL1016", identifier.Location, $"Parameter '{identifier.Text}' is already declared."));
    public void ReportFunctionAlreadyDeclared(SyntaxToken identifier) => Add(new("BL1017", identifier.Location, $"Function '{identifier.Text}' is already declared."));
    public void ReportInvalidBreakOrContinue(SyntaxToken keyword) => Add(new("BL1018", keyword.Location, $"Invalid '{keyword.Text}' outside any loop."));
    public void ReportMainCannotReturnValue(ReturnStatementSyntax returnStatement) => Add(new("BL1019", returnStatement.Location, $"'{GlobalSymbolNames.Main}' cannot return a value."));
    public void ReportReturnMissingValue(TextLocation location, TypeSymbol returnType, string functionName) => Add(new("BL1020", location, $"'{functionName}' needs to return a value of type '{returnType}'."));
    public void ReportReturnTypeMismatch(TextLocation location, TypeSymbol returnType, string functionName, TypeSymbol actualType) => Add(new("1021", location,
        returnType == TypeSymbol.Void
            ? $"'{functionName}' does not have a return type and cannot return a value of type '{actualType}'."
            : $"'{functionName}' needs to return a value of type '{returnType}', not '{actualType}'."));
    public void ReportNotAllPathsReturn(FunctionSymbol function) => Add(new("BL1022", function.Declaration?.Body.ClosedBraceToken.Location ?? default, $"Not all code paths of function '{function.Name}' return a value of type '{function.ReturnType}'."));
    public void ReportInvalidExpressionStatement(TextLocation location) => Add(new("BL1023", location, "Only assignment or call expressions can be used as a statement."));
    public void ReportCannotMixMainAndGlobalStatements(TextLocation location) =>
        Add(new("BL1024", location, $"Global statements cannot be mixed with a '{GlobalSymbolNames.Main}' function."));
    public void ReportInvalidMainSignature(TextLocation location) =>
        Add(new("BL1025", location, $"'{GlobalSymbolNames.Main}' must be parameterless and of type 'void'."));
    public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location) =>
        Add(new("BL1026", location, "At most one file can contain global statements."));
    public void ReportNoEntryPointDefined() =>
        Add(new("BL1027", default, $"No entry point found (neither a '{GlobalSymbolNames.Main}' function nor global statements)."));
    public void ReportInvalidAssemblyReference(string reference, string exceptionMessage) =>
        Add(new("BL2001", default, $"Could not load referenced assembly '{reference}': {exceptionMessage}"));
    public void ReportRequiredTypeNotFound(string metaDataname, TypeSymbol? type = null) =>
        Add(new(
                "BL2001",
                default,
                type is null
                    ? $"The required type '{metaDataname}' could not be resolved among the referenced assemblies."
                    : $"The required type '{type.Name}' ('{metaDataname}') could not be resolved among the referenced assemblies."));
    public void ReportRequiredTypeAmbiguous(string name, TypeDefinition[] typeDefinitions) =>
        Add(new("BL2002", default, $"The required type '{name}' is ambiguous among these referenced assemblies: {string.Join(", ", typeDefinitions.Select(t => t.Module.Assembly.Name.Name).OrderBy(n=>n))}."));
    public void ReportRequiredMethodNotFound(string type, string name, string[] parameterTypeNames) =>
        Add(new("BL2003", default, $"The required method'{type}.{name}({string.Join(", ", parameterTypeNames)})' could not be found in the referenced assemblies."));
}
