﻿using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;
using System.Collections.Generic;
using System.Linq;

namespace Balu;

sealed class DiagnosticBag : List<Diagnostic>
{

    public void ReportUnexpectedToken(int start, int length, string text) =>
        Add(new(Diagnostic.LEX0000, new(start, length), $"Unexpected token '{text}'."));
    public void ReportNumberNotValid(int start, int length, string text) =>
        Add(new(Diagnostic.LEX0001, new(start, length), $"The number '{text}' is not a valid 32bit integer."));
    public void ReportInvalidEscapeSequence(int start, int length, string text) =>
        Add(new(Diagnostic.LEX0002, new(start, length), $"Invalid escape sequence '{text}'."));
    public void ReportUnterminatedString(int start, int length) =>
        Add(new(Diagnostic.LEX0003, new(start, length), "String literal not terminated."));

    public void ReportUnexpectedToken(SyntaxToken foundToken, SyntaxKind expected) =>
        Add(new(Diagnostic.SYX0000, foundToken.Span, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}."));

    public void ReportUnaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol type) =>
        Add(new(Diagnostic.BND0000, operatorToken.Span, $"Unary operator '{operatorToken.Text}' cannot be applied to type '{type.Name}'."));
    public void ReportBinaryOperatorTypeMismatch(SyntaxToken operatorToken, TypeSymbol left, TypeSymbol right) => Add(
        new(Diagnostic.BND0001, operatorToken.Span, $"Binary operator '{operatorToken.Text}' cannot be applied to types '{left.Name}' and '{right.Name}'."));
    public void ReportUndefinedName(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0002, identifierToken.Span, $"Undefined name '{identifierToken.Text}'."));
    public void ReportCannotConvert(TextSpan span, TypeSymbol sourceType, TypeSymbol targetType) => Add(new(Diagnostic.BND0003, span, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'."));
    public void ReportSymbolAlreadyDeclared(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0004, identifierToken.Span, $"Symbol '{identifierToken.Text}' is already declared."));
    public void ReportVariableIsReadOnly(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0005, identifierToken.Span, $"Variable '{identifierToken.Text}' is readonly and cannot be assigned to."));
    public void ReportWrongNumberOfArguments(CallExpressionSyntax syntax, FunctionSymbol function)
    {
        var span = syntax.Span;
        if (syntax.Arguments.ElementsWithSeparators.Any())
        {
            var start = syntax.Arguments.ElementsWithSeparators.First().Span.Start;
            var last = syntax.Arguments.ElementsWithSeparators.Last();
            var end = last.Span.Start + last.Span.Length;
            span = new(start, end - start);
        }
        Add(
            new(Diagnostic.BND0006,
                span,
                $"Function '{syntax.Identifier.Text}' takes {function.Parameters.Length} parameters, but is invoked with {syntax.Arguments.Count} arguments."));
    }
    public void ReportWrongArgumentType(TextSpan span, string functionName, string parameterName, TypeSymbol expectedType, TypeSymbol actualType) => Add(
        new(Diagnostic.BND0007, span,
            $"Parameter '{parameterName}' of function '{functionName}' requires a value of type '{expectedType}', but was given a value of type '{actualType}'."));
    public void ReportExpressionMustHaveValue(TextSpan expressionSpan) => Add(new(Diagnostic.BND0008, expressionSpan,"Expression must return a value."));
    public void ReportInvalidCast(TextSpan span, TypeSymbol fromType, TypeSymbol castType) => Add(new(Diagnostic.BND0009, span, $"No conversion exists from type '{fromType}' to type '{castType}'."));
    public void ReportSymbolNoVariable(SyntaxToken identifier, SymbolKind actual) => Add(new(Diagnostic.BND0010, identifier.Span, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a variable or argument."));
    public void ReportSymbolNoFunction(SyntaxToken identifier, SymbolKind actual) => Add(new(Diagnostic.BND0010, identifier.Span, $"Unexpected symbol kind '{actual}', expected '{identifier.Text}' to be a function."));
    public void ReportUndefinedType(SyntaxToken identifier) => Add(new(Diagnostic.BND0011, identifier.Span, $"Undefined type '{identifier.Text}'."));
    public void ReportUndefinedVariable(SyntaxToken identifier) => Add(new(Diagnostic.BND0012, identifier.Span, $"Undefined variable '{identifier.Text}'."));
    public void ReportUndefinedFunction(SyntaxToken identifier) => Add(new(Diagnostic.BND0013, identifier.Span, $"Undefined function '{identifier.Text}'."));
    public void ReportParameterAlreadyDeclared(SyntaxToken identifier) => Add(new(Diagnostic.BND0014, identifier.Span, $"Parameter '{identifier.Text}' is already declared."));
    public void ReportFunctionAlreadyDeclared(SyntaxToken identifier) => Add(new(Diagnostic.BND0015, identifier.Span, $"Function '{identifier.Text}' is already declared."));


    public void ReportLanguageSupportIssue(TextSpan span, string message) => Add(new(Diagnostic.LNG0001, span, message));
}
