using System.Collections.Generic;
using System.Linq;
using Balu.Text;

namespace Balu.Syntax;

/// <summary>
/// A parser for the Balu language.
/// </summary>
sealed class Parser
{
    readonly DiagnosticBag diagnostics = new();
    readonly List<SyntaxToken> tokens = new();

    int position;

    /// <summary>
    /// The sequence of error messages.
    /// </summary>
    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    /// <summary>
    /// Creates a new <see cref="Parser"/> for the given <paramref name="text"/> of Balu code.
    /// </summary>
    /// <param name="text">The text Balu code to parse.</param>
    public Parser(SourceText text)
    {
        var lexer = new Lexer(text);
        foreach (var token in lexer.Lex().Where(token => token.Kind != SyntaxKind.BadToken && token.Kind != SyntaxKind.WhiteSpaceToken))
        {
            tokens.Add(token);
            if (token.Kind == SyntaxKind.EndOfFileToken) break;
        }

        diagnostics.AddRange(lexer.Diagnostics);
    }

    /// <summary>
    /// Parses the provided text into an <see cref="SyntaxTree"/>.
    /// </summary>
    /// <returns>The resulting <see cref="CompilationUnitSyntax"/> representing the text Balu code.</returns>
    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var statement = ParseStatement();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new(statement, endOfFileToken);
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

    StatementSyntax ParseStatement() => Current.Kind switch
    {
        SyntaxKind.OpenBraceToken => ParseBlockStatement(), 
        SyntaxKind.LetKeyword or
            SyntaxKind.VarKeyword => ParseVariableDeclarationStatement(),
        SyntaxKind.IfKeyword => ParseIfStatement(),
        SyntaxKind.WhileKeyword => ParseWhileStatement(),
        SyntaxKind.ForKeyword => ParseForStatement(),
        _ => ParseExpressionStatement()
    };
    BlockStatementSyntax ParseBlockStatement()
    {
        var open = MatchToken(SyntaxKind.OpenBraceToken);
        var statements = new List<StatementSyntax>();
        while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.ClosedBraceToken)
        {
            var currentToken = Current;
            statements.Add(ParseStatement());
            
            // Check if we consumend a token.
            // If not, we need to skip this, because
            // othwrwise we end up in an infinit loop.
            // Error will be reported by the unfinished
            // statement.
            if (currentToken == Current) NextToken();
        }

        var closed = MatchToken(SyntaxKind.ClosedBraceToken);
        return StatementSyntax.BlockStatement(open, statements, closed);
    }
    ExpressionStatementSyntax ParseExpressionStatement() => StatementSyntax.ExpressionStatement(ParseExpression());
    VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var expectedKeyword = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
        var keyword = MatchToken(expectedKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var expression = ParseExpression();
        return StatementSyntax.VariableDeclarationStatement(keyword, identifier, equals, expression);
    }
    IfStatementSyntax ParseIfStatement()
    {
        var keyword = MatchToken(SyntaxKind.IfKeyword);
        var condition = ParseExpression();
        var thenStatement = ParseStatement();
        ElseClauseSyntax? elseClause = null;
        if (Current.Kind == SyntaxKind.ElseKeyword)
        {
            var elseKeyword = MatchToken(SyntaxKind.ElseKeyword);
            var elseStatement = ParseStatement();
            elseClause = StatementSyntax.Else(elseKeyword, elseStatement);
        }
        return StatementSyntax.IfStatement(keyword, condition, thenStatement, elseClause);
    }
    WhileStatementSyntax ParseWhileStatement()
    {
        var keyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        var statement = ParseStatement();
        return StatementSyntax.WhileStatement(keyword, condition, statement);
    }
    ForStatementSyntax ParseForStatement()
    {
        var keyword = MatchToken(SyntaxKind.ForKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var lowerBound = ParseExpression();
        var toKeyword = MatchToken(SyntaxKind.ToKeyword);
        var upperBound = ParseExpression();
        var body= ParseStatement();
        return StatementSyntax.ForStatement(keyword, identifier, equals, lowerBound, toKeyword, upperBound, body);
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
            SyntaxKind.NumberToken => ParseNumberExpression(),
            SyntaxKind.StringToken => ParseStringExpression(),
            SyntaxKind.OpenParenthesisToken => ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword or
                SyntaxKind.FalseKeyword => ParseBooleanExpression(),
            SyntaxKind.IdentifierToken => ParseIdentifier(),
            _ => ParseNameExpression()
        };
    LiteralExpressionSyntax ParseNumberExpression()
    {
        var numberToken = MatchToken(SyntaxKind.NumberToken);
        return new(numberToken);
    }
    LiteralExpressionSyntax ParseStringExpression()
    {
        var stringToken = MatchToken(SyntaxKind.StringToken);
        return new(stringToken);
    }
    ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        var left = MatchToken(SyntaxKind.OpenParenthesisToken);
        var expression = ParseExpression();
        var right = MatchToken(SyntaxKind.ClosedParenthesisToken);
        return new (left, expression, right);
    }
    LiteralExpressionSyntax ParseBooleanExpression()
    {
        var value = Current.Kind == SyntaxKind.TrueKeyword;
        var token = MatchToken(value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);
        return new (token, value);
    }
    ExpressionSyntax ParseIdentifier()
    {
        if (Current.Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
            return ParseCallExpression();
        return ParseNameExpression();
    }
    CallExpressionSyntax ParseCallExpression()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var open = MatchToken(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseArguments();
        var closed = MatchToken(SyntaxKind.ClosedParenthesisToken);
        return new(identifier, open, arguments, closed);
    }
    SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
    {
        List<SyntaxNode> arguments = new();
        while(Current.Kind != SyntaxKind.ClosedParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            arguments.Add(ParseExpression());
            if (Current.Kind != SyntaxKind.ClosedParenthesisToken)
            {
                var comma = MatchToken(SyntaxKind.CommaToken);
                arguments.Add(comma);
                if (Current.Kind == SyntaxKind.ClosedParenthesisToken)
                    diagnostics.ReportUnexpectedToken(comma, SyntaxKind.ClosedBraceToken);
            }
        }
        return new(arguments);
    }
    NameExpressionSyntax ParseNameExpression() => new(MatchToken(SyntaxKind.IdentifierToken));
    SyntaxToken MatchToken(SyntaxKind kind)
    {
        if (Current.Kind == kind)
            return NextToken();

        diagnostics.ReportUnexpectedToken(Current, kind);
        return new(kind, Current.Span);
    }
}
