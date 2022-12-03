using System.Collections.Generic;
using System.Linq;

namespace Balu.Syntax;

/// <summary>
/// A parser for the Balu language.
/// </summary>
sealed class Parser
{
    readonly List<Diagnostic> diagnostics = new();
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
        diagnostics.Clear();
        tokens.Clear();
        position = 0;

        var lexer = new Lexer(input);
        foreach (var token in lexer.Lex().Where(token => token.Kind != SyntaxKind.BadToken && token.Kind != SyntaxKind.WhiteSpaceToken))
        {
            tokens.Add(token);
            if (token.Kind == SyntaxKind.EndOfFileToken) break;
        }
        diagnostics.AddRange(lexer.Diagnostics);

        var expresion = ParseExpression();
        var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
        return new(expresion, endOfFileToken, diagnostics);
    }

    SyntaxToken Current => position < tokens.Count ? tokens[position] : tokens[^1];
    SyntaxToken NextToken()
    {
        var current = Current;
        if (position < tokens.Count) position++;
        return current;
    }

    ExpressionSyntax ParseExpression() => ParseAssignmentExpression();
    ExpressionSyntax ParseAssignmentExpression() => ParseBinaryExpression();
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
            _ => ParseNumberExpression()
        };
    ExpressionSyntax ParseNumberExpression()
    {
        var numberToken = Match(SyntaxKind.NumberToken);
        return new LiteralExpressionSyntax(numberToken);
    }
    ExpressionSyntax ParseParenthesizedExpression()
    {
        var left = NextToken();
        var expression = ParseExpression();
        var right = Match(SyntaxKind.ClosedParenthesisToken);
        return new ParenthesizedExpressionSyntax(left, expression, right);
    }
    ExpressionSyntax ParseBooleanExpression()
    {
        var token = NextToken();
        return new LiteralExpressionSyntax(token, token.Kind == SyntaxKind.TrueKeyword);
    }
    SyntaxToken Match(SyntaxKind kind)
    {
        if (Current.Kind == kind)
            return NextToken();

        diagnostics.Add(Diagnostic.ParserUnexpectedToken(Current, kind));
        return new(kind, Current.TextSpan);
    }
}
