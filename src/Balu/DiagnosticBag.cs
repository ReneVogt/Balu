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

    public void ReportUnexpectedToken(TextLocation location, string text) =>
        Add(new(Diagnostic.LEX0000, location, $"Unexpected token '{text}'."));
    public void ReportNumberNotValid(TextLocation location, string text) =>
        Add(new(Diagnostic.LEX0001, location, $"The number '{text}' is not a valid 32bit integer."));
    public void ReportInvalidEscapeSequence(TextLocation location, string text) =>
        Add(new(Diagnostic.LEX0002, location, $"Invalid escape sequence '{text}'."));
    public void ReportUnterminatedString(TextLocation location) =>
        Add(new(Diagnostic.LEX0003, location, "String literal not terminated."));

    public void ReportUnexpectedToken(SyntaxToken foundToken, SyntaxKind expected) =>
        Add(new(Diagnostic.SYX0000, foundToken.Location, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}."));

    public void ReportUnaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol type) =>
        Add(new(Diagnostic.BND0000, operatorToken.Location, $"Unary operator '{operatorToken.Text}' cannot be applied to type '{type.Name}'."));
    public void ReportBinaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol left, TypeSymbol right) => Add(
        new(Diagnostic.BND0001, operatorToken.Location, $"Binary operator '{operatorToken.Text}' cannot be applied to types '{left.Name}' and '{right.Name}'."));
    public void ReportUndefinedName(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0002, identifierToken.Location, $"Undefined name '{identifierToken.Text}'."));
    public void ReportCannotConvert(TextLocation location, TypeSymbol sourceType, TypeSymbol targetType) => Add(new(Diagnostic.BND0003, location, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'."));
    public void ReportCannotConvertImplicit(TextLocation location, TypeSymbol sourceType, TypeSymbol targetType) => Add(new(Diagnostic.BND0003, location, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'. An explicit conversion exists, are you missing a cast?"));
    public void ReportSymbolAlreadyDeclared(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0004, identifierToken.Location, $"Symbol '{identifierToken.Text}' is already declared."));
    public void ReportVariableIsReadOnly(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0005, identifierToken.Location, $"Variable '{identifierToken.Text}' is readonly and cannot be assigned to."));
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
            new(Diagnostic.BND0006,
                location,
                $"Function '{syntax.Identifier.Text}' takes {function.Parameters.Length} parameters, but is invoked with {syntax.Arguments.Count} arguments."));
    }
    public void ReportWrongArgumentType(TextLocation location, string functionName, string parameterName, TypeSymbol expectedType, TypeSymbol actualType) => Add(
        new(Diagnostic.BND0007, location,
            $"Parameter '{parameterName}' of function '{functionName}' requires a value of type '{expectedType}', but was given a value of type '{actualType}'."));
    public void ReportExpressionMustHaveValue(TextLocation location) => Add(new(Diagnostic.BND0008, location,"Expression must return a value."));
    public void ReportSymbolNoVariable(SyntaxToken identifier, SymbolKind actual) => Add(new(Diagnostic.BND0010, identifier.Location, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a variable or argument."));
    public void ReportSymbolNoFunction(SyntaxToken identifier, SymbolKind actual) => Add(new(Diagnostic.BND0010, identifier.Location, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a function."));
    public void ReportUndefinedType(SyntaxToken identifier) => Add(new(Diagnostic.BND0011, identifier.Location, $"Undefined type '{identifier.Text}'."));
    public void ReportUndefinedVariable(SyntaxToken identifier) => Add(new(Diagnostic.BND0012, identifier.Location, $"Undefined variable '{identifier.Text}'."));
    public void ReportUndefinedFunction(SyntaxToken identifier) => Add(new(Diagnostic.BND0013, identifier.Location, $"Undefined function '{identifier.Text}'."));
    public void ReportParameterAlreadyDeclared(SyntaxToken identifier) => Add(new(Diagnostic.BND0014, identifier.Location, $"Parameter '{identifier.Text}' is already declared."));
    public void ReportFunctionAlreadyDeclared(SyntaxToken identifier) => Add(new(Diagnostic.BND0015, identifier.Location, $"Function '{identifier.Text}' is already declared."));
    public void ReportInvalidBreakOrContinue(SyntaxToken keyword) => Add(new(Diagnostic.BND0016, keyword.Location, $"Invalid '{keyword.Text}' outside any loop."));
    public void ReportMainCannotReturnValue(ReturnStatementSyntax returnStatement) => Add(new(Diagnostic.BND0017, returnStatement.Location, "'main' cannot return a value."));
    public void ReportReturnMissingValue(TextLocation location, TypeSymbol returnType, string functionName) => Add(new(Diagnostic.BND0018, location, $"'{functionName}' needs to return a value of type '{returnType}'."));
    public void ReportReturnTypeMismatch(TextLocation location, TypeSymbol returnType, string functionName, TypeSymbol actualType) => Add(new(Diagnostic.BND0019, location,
        returnType == TypeSymbol.Void
            ? $"'{functionName}' does not have a return type and cannot return a value of type '{actualType}'."
            : $"'{functionName}' needs to return a value of type '{returnType}', not '{actualType}'."));
    public void ReportNotAllPathsReturn(FunctionSymbol function) => Add(new(Diagnostic.BND0020, function.Declaration!.Identifier.Location, $"Not all code paths of function '{function.Name}' return a value of type '{function.ReturnType}'."));
    public void ReportInvalidExpressionStatement(TextLocation location) => Add(new(Diagnostic.BND0021, location, "Only assignment or call expressions can be used as a statement."));
    public void ReportCannotMixMainAndGlobalStatements(TextLocation location) =>
        Add(new(Diagnostic.BND0022, location, "Global statements cannot be mixed with a main function."));
    public void ReportInvalidMainSignature(TextLocation location) =>
        Add(new(Diagnostic.BND0023, location, "'main' must be parameterless and of type 'void'."));
    public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location) =>
        Add(new(Diagnostic.BND0024, location, "At most one file can contain global statements."));
    public void ReportNoEntryPointDefined() =>
        Add(new(Diagnostic.BND0025, default, "No entry point found (neither a 'main' function nor global statements)."));
    public void ReportInvalidAssemblyReference(string reference, string exceptionMessage) =>
        Add(new(Diagnostic.ILE0001, default, $"Could not load referenced assembly '{reference}': {exceptionMessage}"));
    public void ReportRequiredTypeNotFound(string metaDataname, TypeSymbol? type = null) =>
        Add(new(
                Diagnostic.ILE0002,
                default,
                type is null
                    ? $"The required type '{metaDataname}' could not be resolved among the referenced assemblies."
                    : $"The required type '{type.Name}' ('{metaDataname}') could not be resolved among the referenced assemblies."));
    public void ReportRequiredTypeAmbiguous(string name, TypeDefinition[] typeDefinitions) =>
        Add(new(Diagnostic.ILE0003, default, $"The required type '{name}' is ambiguous among these referenced assemblies: {string.Join(", ", typeDefinitions.Select(t => t.Module.Assembly.Name.Name).OrderBy(n=>n))}."));
    public void ReportRequiredMethodNotFound(string type, string name, string[] parameterTypeNames) =>
        Add(new(Diagnostic.ILE0004, default, $"The required method'{type}.{name}({string.Join(", ", parameterTypeNames)}' could not be found in the referenced assemblies."));
}
