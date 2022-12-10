﻿using Balu.Syntax;
using Balu.Text;
using System;
using System.Collections.Generic;

namespace Balu;

sealed class DiagnosticBag : List<Diagnostic>
{

    public void ReportUnexpectedToken(int start, int length, string text) =>
        Add(new(Diagnostic.LEX0000, new(start, length), $"Unexpected token '{text}'."));
    public void ReportNumberNotValid(int start, int length, string text) =>
        Add(new(Diagnostic.LEX0001, new(start, length), $"The number '{text}' is not a valid 32bit integer."));

    public void ReportUnexpectedToken(SyntaxToken foundToken, SyntaxKind expected) =>
        Add(new(Diagnostic.SYX0000, foundToken.Span, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}."));

    public void ReportUnaryOperatorTypeMismatch(SyntaxToken operatorToken, Type type) =>
        Add(new(Diagnostic.BND0000, operatorToken.Span, $"Unary operator '{operatorToken.Text}' cannot be applied to type '{type.Name}'."));
    public void ReportBinaryOperatorTypeMismatch(SyntaxToken operatorToken, Type left, Type right) => Add(
        new(Diagnostic.BND0001, operatorToken.Span, $"Binary operator '{operatorToken.Text}' cannot be applied to types '{left.Name}' and '{right.Name}'."));
    public void ReportUndefinedName(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0002, identifierToken.Span, $"Undefined name '{identifierToken.Text}'."));
    public void ReportCannotConvert(TextSpan span, Type sourceType, Type targetType) => Add(new(Diagnostic.BND0003, span, $"Cannot convert '{sourceType.Name}' to '{targetType.Name}'."));
    public void ReportVariableAlreadyDeclared(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0004, identifierToken.Span, $"Variable '{identifierToken.Text}' is already declared."));
    public void ReportVariableIsReadOnly(SyntaxToken identifierToken) => Add(new(Diagnostic.BND0004, identifierToken.Span, $"Variable '{identifierToken.Text}' is readonly and cannot be assigned to."));
}
