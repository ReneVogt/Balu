using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        var members = ParseMembers();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new(members, endOfFileToken);
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

    ImmutableArray<MemberSyntax> ParseMembers()
    {
        var members = ImmutableArray.CreateBuilder<MemberSyntax>();
        while (Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var currentToken = Current;
            members.Add(ParseMember());

            // Check if we consumend a token.
            // If not, we need to skip this, because
            // othwrwise we end up in an infinit loop.
            // Error will be reported by the unfinished
            // statement.
            if (currentToken == Current) NextToken();
        }

        return members.ToImmutable();
    }
    MemberSyntax ParseMember() => Current.Kind switch
    {
        SyntaxKind.FunctionKeyword => ParseFunctionDeclaration(),
        _ => ParseGlobalStatement()
    };
    GlobalStatementSyntax ParseGlobalStatement() => new (ParseStatement());
    FunctionDeclarationSyntax ParseFunctionDeclaration()
    {
        var keyword = MatchToken(SyntaxKind.FunctionKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameters();
        var closedParenthesis = MatchToken(SyntaxKind.ClosedParenthesisToken);
        var type = ParseOptionalTypeClause();
        var body = ParseBlockStatement();
        return MemberSyntax.FunctionDeclaration(keyword, identifier, openParenthesis, parameters, closedParenthesis, type, body);
    }
    SeparatedSyntaxList<ParameterSyntax> ParseParameters()
    {
        if (Current.Kind == SyntaxKind.ClosedParenthesisToken) return new(ImmutableArray<SyntaxNode>.Empty);

        var parameters = ImmutableArray.CreateBuilder<SyntaxNode>();
        while (Current.Kind != SyntaxKind.ClosedParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            parameters.Add(ParseParameter());
            if (Current.Kind != SyntaxKind.ClosedParenthesisToken)
            {
                var comma = MatchToken(SyntaxKind.CommaToken);
                parameters.Add(comma);
                if (Current.Kind == SyntaxKind.ClosedParenthesisToken)
                    diagnostics.ReportUnexpectedToken(comma, SyntaxKind.ClosedBraceToken);
            }
        }
        return new(parameters.ToImmutable());
    }
    ParameterSyntax ParseParameter()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var type = ParseTypeClause();
        return SyntaxNode.Parameter(identifier, type);
    }
    StatementSyntax ParseStatement() => Current.Kind switch
    {
        SyntaxKind.OpenBraceToken => ParseBlockStatement(), 
        SyntaxKind.LetKeyword or
            SyntaxKind.VarKeyword => ParseVariableDeclarationStatement(),
        SyntaxKind.IfKeyword => ParseIfStatement(),
        SyntaxKind.WhileKeyword => ParseWhileStatement(),
        SyntaxKind.DoKeyword => ParseDoWhileStatement(),
        SyntaxKind.ForKeyword => ParseForStatement(),
        SyntaxKind.ContinueKeyword => ParseContinueStatement(),
        SyntaxKind.BreakKeyword=> ParseBreakStatement(),
        SyntaxKind.ReturnKeyword => ParseReturnStatement(),
        _ => ParseExpressionStatement()
    };
    BlockStatementSyntax ParseBlockStatement()
    {
        var open = MatchToken(SyntaxKind.OpenBraceToken);
        var statementsBuilder = ImmutableArray.CreateBuilder<StatementSyntax>();
        while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.ClosedBraceToken)
        {
            var currentToken = Current;
            statementsBuilder.Add(ParseStatement());
            
            // Check if we consumend a token.
            // If not, we need to skip this, because
            // othwrwise we end up in an infinit loop.
            // Error will be reported by the unfinished
            // statement.
            if (currentToken == Current) NextToken();
        }

        var closed = MatchToken(SyntaxKind.ClosedBraceToken);
        return StatementSyntax.BlockStatement(open, statementsBuilder.ToImmutable(), closed);
    }
    ExpressionStatementSyntax ParseExpressionStatement() => StatementSyntax.ExpressionStatement(ParseExpression());
    VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var expectedKeyword = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
        var keyword = MatchToken(expectedKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeClause = ParseOptionalTypeClause();
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var expression = ParseExpression();
        return StatementSyntax.VariableDeclarationStatement(keyword, identifier, equals, expression, typeClause);
    }
    ContinueStatementSyntax ParseContinueStatement()
    {
        var keyword = MatchToken(SyntaxKind.ContinueKeyword);
        return StatementSyntax.ContinueStatement(keyword);
    }
    BreakStatementSyntax ParseBreakStatement()
    {
        var keyword = MatchToken(SyntaxKind.BreakKeyword);
        return StatementSyntax.BreakStatement(keyword);
    }
    ReturnStatementSyntax ParseReturnStatement()
    {
        throw new NotImplementedException();
    }
    TypeClauseSyntax? ParseOptionalTypeClause()
    {
        if (Current.Kind != SyntaxKind.ColonToken) return null;
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new(colonToken, identifier);
    }
    TypeClauseSyntax ParseTypeClause()
    {
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new(colonToken, identifier);
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
            elseClause = SyntaxNode.Else(elseKeyword, elseStatement);
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
    DoWhileStatementSyntax ParseDoWhileStatement()
    {
        var doKeyword = MatchToken(SyntaxKind.DoKeyword);
        var statement = ParseStatement();
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        return StatementSyntax.DoWhileStatement(doKeyword, statement, whileKeyword, condition);
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
        var argumentsBuilder = ImmutableArray.CreateBuilder<SyntaxNode>();
        var parseNextArgument = true;
        while(parseNextArgument && Current.Kind != SyntaxKind.ClosedParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            argumentsBuilder.Add(ParseExpression());
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                var comma = MatchToken(SyntaxKind.CommaToken);
                argumentsBuilder.Add(comma);
            }
            else parseNextArgument = false;
        }
        if (argumentsBuilder.Count % 2 == 0 && argumentsBuilder.Count > 0)
            diagnostics.ReportUnexpectedToken((SyntaxToken)argumentsBuilder.Last(), SyntaxKind.ClosedParenthesisToken);
        return new(argumentsBuilder.ToImmutable());
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
