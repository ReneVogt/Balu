using System;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using System.Collections.Generic;
using System.Linq;

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
    public void ReportReturnOutsideOfFunction(ReturnStatementSyntax returnStatement) => Add(new(Diagnostic.BND0017, returnStatement.ReturnKeyword.Location, $"The '{returnStatement.ReturnKeyword.Kind.GetText()}' keyword can only be used in functions."));
    public void ReportReturnMissingValue(TextLocation location, FunctionSymbol function) => Add(new(Diagnostic.BND0018, location, $"'{function.Name}' needs to return a value of type '{function.ReturnType}'."));
    public void ReportReturnTypeMismatch(TextLocation location, FunctionSymbol function, TypeSymbol actualType) => Add(new(Diagnostic.BND0019, location,
        function.ReturnType == TypeSymbol.Void
            ? $"'{function.Name}' does not have a return type and cannot return a value of type '{actualType}'."
            : $"'{function.Name}' needs to return a value of type '{function.ReturnType}', not '{actualType}'."));
    public void ReportNotAllPathsReturn(FunctionSymbol function) => Add(new(Diagnostic.BND0020, function.Declaration!.Identifier.Location, $"Not all code paths of function '{function.Name}' return a value of type '{function.ReturnType}'."));
}
