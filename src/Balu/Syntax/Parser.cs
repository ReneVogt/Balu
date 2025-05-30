﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Balu.Diagnostics;
using Balu.Text;

namespace Balu.Syntax;

sealed class Parser
{
    readonly DiagnosticBag diagnostics = [];
    readonly List<SyntaxToken> tokens = [];
    readonly SourceText sourceText;
    readonly SyntaxTree syntaxTree;

    int position;

    public IEnumerable<Diagnostic> Diagnostics => diagnostics;

    public Parser(SyntaxTree syntaxTree)
    {
        this.syntaxTree = syntaxTree;
        sourceText = this.syntaxTree.Text;

        var badTokens = new List<SyntaxToken>();

        var lexer = new Lexer(syntaxTree);
        foreach (var token in lexer.Lex())
        {
            if (token.Kind == SyntaxKind.BadToken)
                badTokens.Add(token);
            else
            {
                if (badTokens.Count == 0)
                    tokens.Add(token);
                else
                {
                    var triviaBuilder = token.LeadingTrivia.ToBuilder();
                    int index = 0;
                    foreach (var badToken in badTokens)
                    {
                        foreach (var trivia in badToken.LeadingTrivia)
                            triviaBuilder.Insert(index++, trivia);
                        triviaBuilder.Insert(index++, new (token.SyntaxTree, SyntaxKind.SkippedTextTrivia, badToken.Text, badToken.Span));
                        foreach (var trivia in badToken.TrailingTrivia)
                            triviaBuilder.Insert(index++, trivia);
                    }
                    tokens.Add(new(token.SyntaxTree, token.Kind, token.Span, token.Text, token.Value, triviaBuilder.ToImmutable(),
                                   token.TrailingTrivia));
                    badTokens.Clear();
                }
                if (token.Kind == SyntaxKind.EndOfFileToken) break;
            }
        }

        diagnostics.AddRange(lexer.Diagnostics);
    }

    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var members = ParseMembers();
        var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new(syntaxTree, members, endOfFileToken);
    }

    SyntaxToken Peek(int offset)
    {
        int index = position + offset;
        return index >= tokens.Count ? tokens.Last() : tokens[index];
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
            if (currentToken == Current) 
                SkipToken();
        }

        return members.ToImmutable();
    }

    MemberSyntax ParseMember() => Current.Kind switch
    {
        SyntaxKind.FunctionKeyword => ParseFunctionDeclaration(),
        _ => ParseGlobalStatement()
    };
    GlobalStatementSyntax ParseGlobalStatement() => new (syntaxTree, ParseStatement());
    FunctionDeclarationSyntax ParseFunctionDeclaration()
    {
        var keyword = MatchToken(SyntaxKind.FunctionKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameters();
        var closedParenthesis = MatchToken(SyntaxKind.ClosedParenthesisToken);
        var type = ParseOptionalTypeClause();
        var body = ParseBlockStatement();   
        return new(syntaxTree, keyword, identifier, openParenthesis, parameters, closedParenthesis, type, body);
    }
    SeparatedSyntaxList<ParameterSyntax> ParseParameters() => ParseSeparatedSyntaxList(ParseParameter);
    ParameterSyntax ParseParameter()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var type = ParseTypeClause();
        return new(syntaxTree, identifier, type);
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
            if (currentToken == Current) 
                SkipToken();
        }

        var closed = MatchToken(SyntaxKind.ClosedBraceToken);
        return new(syntaxTree, open, statementsBuilder.ToImmutable(), closed);
    }
    ExpressionStatementSyntax ParseExpressionStatement() => new(syntaxTree, ParseExpression());
    VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var expectedKeyword = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
        var keyword = MatchToken(expectedKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeClause = ParseOptionalTypeClause();
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var expression = ParseExpression();
        return new(syntaxTree, keyword, identifier, equals, expression, typeClause);
    }
    ContinueStatementSyntax ParseContinueStatement()
    {
        var keyword = MatchToken(SyntaxKind.ContinueKeyword);
        return new(syntaxTree, keyword);
    }
    BreakStatementSyntax ParseBreakStatement()
    {
        var keyword = MatchToken(SyntaxKind.BreakKeyword);
        return new(syntaxTree, keyword);
    }
    ReturnStatementSyntax ParseReturnStatement()
    {
        var keyword = MatchToken(SyntaxKind.ReturnKeyword);
        return new(syntaxTree,
                   keyword,
                   Current.Kind == SyntaxKind.EndOfFileToken || sourceText.GetLineIndex(keyword.Span.Start) != sourceText.GetLineIndex(Current.Span.Start)
                       ? null
                       : ParseExpression());
    }
    TypeClauseSyntax? ParseOptionalTypeClause()
    {
        if (Current.Kind != SyntaxKind.ColonToken) return null;
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new(syntaxTree, colonToken, identifier);
    }
    TypeClauseSyntax ParseTypeClause()
    {
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new(syntaxTree, colonToken, identifier);
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
            elseClause = new(syntaxTree, elseKeyword, elseStatement);
        }
        return new(syntaxTree, keyword, condition, thenStatement, elseClause);
    }
    WhileStatementSyntax ParseWhileStatement()
    {
        var keyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        var statement = ParseStatement();
        return new(syntaxTree, keyword, condition, statement);
    }
    DoWhileStatementSyntax ParseDoWhileStatement()
    {
        var doKeyword = MatchToken(SyntaxKind.DoKeyword);
        var statement = ParseStatement();
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        return new(syntaxTree, doKeyword, statement, whileKeyword, condition);
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
        return new(syntaxTree, keyword, identifier, equals, lowerBound, toKeyword, upperBound, body);
    }

    ExpressionSyntax ParseExpression()
    {
        if (Current.Kind == SyntaxKind.IdentifierToken &&Peek(1).Kind.IsAssingmentToken()) 
            return ParseAssignmentExpression();
        return ParseBinaryExpression();
    }
    AssignmentExpressionSyntax ParseAssignmentExpression() => new(syntaxTree, NextToken(), NextToken(),
                                                                                         ParseExpression());
    PrefixExpressionSyntax ParsePrefixExpression()
    {
        var operatorToken = Current.Kind == SyntaxKind.MinusMinusToken
                                ? MatchToken(SyntaxKind.MinusMinusToken)
                                : MatchToken(SyntaxKind.PlusPlusToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new PrefixExpressionSyntax(syntaxTree, operatorToken, identifier);
    }
    PostfixExpressionSyntax ParsePostfixExpression()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var operatorToken = Current.Kind == SyntaxKind.MinusMinusToken
                                ? MatchToken(SyntaxKind.MinusMinusToken)
                                : MatchToken(SyntaxKind.PlusPlusToken);
        return new PostfixExpressionSyntax(syntaxTree, identifier, operatorToken);
    }
    ExpressionSyntax ParseBinaryExpression(int parentprecedence = 0)
    {
        var unaryOperatorPrecedence = Current.Kind.UnaryOperatorPrecedence();
        var left = unaryOperatorPrecedence > 0 && unaryOperatorPrecedence >= parentprecedence
                       ? new UnaryExpressionSyntax(syntaxTree, NextToken(), ParseBinaryExpression(unaryOperatorPrecedence))
                       : ParsePrimaryExpression();
        for (; ; )
        {
            var precedence = Current.Kind.BinaryOperatorPrecedence();
            if (precedence <= parentprecedence) return left;

            var operatorToken = NextToken();
            var right = ParseBinaryExpression(precedence);
            left = new BinaryExpressionSyntax(syntaxTree, left, operatorToken, right);
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
            SyntaxKind.PlusPlusToken or 
                SyntaxKind.MinusMinusToken => ParsePrefixExpression(),
            _ => ParseNameExpression()
        };
    LiteralExpressionSyntax ParseNumberExpression()
    {
        var numberToken = MatchToken(SyntaxKind.NumberToken);
        return new(syntaxTree, numberToken);
    }
    LiteralExpressionSyntax ParseStringExpression()
    {
        var stringToken = MatchToken(SyntaxKind.StringToken);
        return new(syntaxTree, stringToken);
    }
    ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        var left = MatchToken(SyntaxKind.OpenParenthesisToken);
        var expression = ParseExpression();
        var right = MatchToken(SyntaxKind.ClosedParenthesisToken);
        return new(syntaxTree, left, expression, right);
    }
    LiteralExpressionSyntax ParseBooleanExpression()
    {
        var value = Current.Kind == SyntaxKind.TrueKeyword;
        var token = MatchToken(value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);
        return new (syntaxTree, token, value);
    }
    ExpressionSyntax ParseIdentifier()
    {
        if (Current.Kind == SyntaxKind.IdentifierToken)
        {
            var  next = Peek(1).Kind;
            if (next == SyntaxKind.OpenParenthesisToken)
                return ParseCallExpression();
            if (next.IsPreOrPostfixToken())
                return ParsePostfixExpression();
        }

        return ParseNameExpression();
    }
    CallExpressionSyntax ParseCallExpression()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var open = MatchToken(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseArguments();
        var closed = MatchToken(SyntaxKind.ClosedParenthesisToken);
        return new(syntaxTree, identifier, open, arguments, closed);
    }
    SeparatedSyntaxList<ExpressionSyntax> ParseArguments() => ParseSeparatedSyntaxList(ParseExpression);
    SeparatedSyntaxList<T> ParseSeparatedSyntaxList<T>(Func<T> parseMethod) where T : SyntaxNode
    {
        var listBuilder = ImmutableArray.CreateBuilder<SyntaxNode>();
        var parseNextElement = true;
        while (parseNextElement && Current.Kind != SyntaxKind.ClosedParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            listBuilder.Add(parseMethod());
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                var comma = MatchToken(SyntaxKind.CommaToken);
                listBuilder.Add(comma);
            }
            else parseNextElement = false;
        }
        if (listBuilder.Count % 2 == 0 && listBuilder.Count > 0)
            diagnostics.ReportUnexpectedToken((SyntaxToken)listBuilder.Last(), SyntaxKind.ClosedParenthesisToken);
        return new(listBuilder.ToImmutable());
    }
    NameExpressionSyntax ParseNameExpression() => new(syntaxTree, MatchToken(SyntaxKind.IdentifierToken));
    SyntaxToken MatchToken(SyntaxKind kind)
    {
        if (Current.Kind == kind)
            return NextToken();

        diagnostics.ReportUnexpectedToken(Current, kind);
        return new(syntaxTree, kind, Current.Span, null, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
    }
    void SkipToken()
    {
        var skipped = Current;
        NextToken();
        var triviaBuilder = Current.LeadingTrivia.ToBuilder();
        int index = 0;
        foreach (var t in skipped.LeadingTrivia)
            triviaBuilder.Insert(index++, t);
        triviaBuilder.Insert(index++, new(syntaxTree, SyntaxKind.SkippedTextTrivia, skipped.Text, skipped.Span));
        foreach (var t in skipped.TrailingTrivia)
            triviaBuilder.Insert(index++, t);
        tokens[position] = new(syntaxTree, Current.Kind, Current.Span, Current.Text, Current.Value, triviaBuilder.ToImmutable(),
                               Current.TrailingTrivia);
    }
}
