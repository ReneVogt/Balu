using Balu.Syntax;
using Balu.Text;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Balu;
sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    readonly List<Diagnostic> diagnostics = new();

    public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void ReportUnexpectedToken(int start, int length, string text) =>
        diagnostics.Add(new(Diagnostic.LEX0000, new(start, length), $"Unexpected token '{text}'."));
    public void ReportNumberNotValid(int start, int length, string text) =>
        diagnostics.Add(new(Diagnostic.LEX0001, new(start, length), $"The number '{text}' is not a valid 32bit integer."));

    public void ReportUnexpectedToken(SyntaxToken foundToken, SyntaxKind expected) =>
        diagnostics.Add(new(Diagnostic.SYX0000, foundToken.Span, $"Unexpected {foundToken.Kind} ('{foundToken.Text}'), expected {expected}."));
    
    public void ReportUnaryOperatorTypeMismatch(SyntaxToken operatorToken, Type type) =>
        diagnostics.Add(new(Diagnostic.BND0000, operatorToken.Span, $"Unary operator '{operatorToken.Text}' cannot be applied to type '{type}'."));
    public void ReportBinaryOperatorTypeMismatch(SyntaxToken operatorToken, Type left, Type right) => diagnostics.Add(
        new(Diagnostic.BND0001, operatorToken.Span, $"Binary operator '{operatorToken.Text}' cannot be applied to types '{left}' and '{right}'."));
    public void ReportUndefinedName(string name, TextSpan span) => diagnostics.Add(new("BND0002", span, $"Undefined name '{name}'."));
}
