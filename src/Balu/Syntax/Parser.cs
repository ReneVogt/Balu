using System.Collections.Generic;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// A parser for the Balu language.
/// </summary>
sealed class Parser
{
    readonly DiagnosticBag diagnostics = new();
    readonly List<SyntaxToken> tokens = new();
    readonly string input;

    int position;

    /// <summary>
    /// The sequence of error messages.
    /// </summary>
    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    /// <summary>
    /// Creates a new <see cref="Parser"/> for the given <paramref name="input"/> of Balu code.
    /// </summary>
    /// <param name="input">The input Balu code to parse.</param>
    public Parser(string input) => this.input = input;

    /// <summary>
    /// Parses the provided input into an <see cref="SyntaxTree"/>.
    /// </summary>
    /// <returns>The resulting <see cref="SyntaxTree"/> representing the input Balu code.</returns>
    public SyntaxTree Parse()
    {
        tokens.Clear();
        position = 0;

        var lexer = new Lexer(input);
        foreach (var token in lexer.Lex().Where(token => token.Kind != SyntaxKind.BadToken && token.Kind != SyntaxKind.WhiteSpaceToken))
        {
            tokens.Add(token);
            if (token.Kind == SyntaxKind.EndOfFileToken) break;
        }

        var expresion = ParseExpression();
        var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
        return new(expresion, endOfFileToken, lexer.Diagnostics.Concat(diagnostics));
    }

    SyntaxToken Peek(int offset)
    {
        int index = position + offset;
        return index >= tokens.Count ? tokens[^1] : tokens[index];
    }
    SyntaxToken Current => Peek(0);
    SyntaxToken NextToken()
    {
        var current = Current;
        if (position < tokens.Count) position++;
        return current;
    }

    ExpressionSyntax ParseExpression() => ParseAssignmentExpression();
    ExpressionSyntax ParseAssignmentExpression() =>
        Current.Kind != SyntaxKind.IdentifierToken || Peek(1).Kind != SyntaxKind.EqualsToken
            ? ParseBinaryExpression()
            : new AssignmentExpressionSyntax(NextToken(), NextToken(), ParseAssignmentExpression());
    ExpressionSyntax ParseBinaryExpression(int parentprecedence = 0)
    {
        var unaryOperatorPrecedence = Current.Kind.UnaryOperatorPrecedence();
        var left = unaryOperatorPrecedence > 0 && unaryOperatorPrecedence >= parentprecedence
                       ? ExpressionSyntax.Unary(NextToken(), ParseBinaryExpression(unaryOperatorPrecedence))
                       : ParsePrimaryExpression();
        for (; ; )
        {
            var precedence = Current.Kind.BinaryOperatorPrecedence();
            if (precedence <= parentprecedence) return left;

            var operatorToken = NextToken();
            var right = ParseBinaryExpression(precedence);
            left = ExpressionSyntax.Binary(left, operatorToken, right);
        }
    }
    ExpressionSyntax ParsePrimaryExpression() =>
        Current.Kind switch
        {
            SyntaxKind.OpenParenthesisToken => ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword or
                SyntaxKind.FalseKeyword => ParseBooleanExpression(),
            SyntaxKind.IdentifierToken => ParseNameExpression(),
            _ => ParseNumberExpression()
        };
    LiteralExpressionSyntax ParseNumberExpression()
    {
        var numberToken = Match(SyntaxKind.NumberToken);
        return new (numberToken);
    }
    ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        var left = NextToken();
        var expression = ParseExpression();
        var right = Match(SyntaxKind.ClosedParenthesisToken);
        return new (left, expression, right);
    }
    LiteralExpressionSyntax ParseBooleanExpression()
    {
        var token = NextToken();
        return new (token, token.Kind == SyntaxKind.TrueKeyword);
    }
    NameExpressionSyntax ParseNameExpression() => new(NextToken());
    SyntaxToken Match(SyntaxKind kind)
    {
        if (Current.Kind == kind)
            return NextToken();

        diagnostics.ReportUnexpectedToken(Current, kind);
        return new(kind, Current.TextSpan);
    }
}
