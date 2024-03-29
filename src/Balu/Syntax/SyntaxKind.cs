﻿namespace Balu.Syntax;

public enum SyntaxKind
{
    #region Trivia
    SkippedTextTrivia,
    WhiteSpaceTrivia,
    LineBreakTrivia,
    SingleLineCommentTrivia,
    MultiLineCommentTrivia,
    #endregion
    #region Tokens
    BadToken,
    EndOfFileToken,

    NumberToken,
    StringToken,

    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,

    OpenParenthesisToken,
    ClosedParenthesisToken,

    OpenBraceToken,
    ClosedBraceToken,

    EqualsToken,
    PlusEqualsToken,
    MinusEqualsToken,
    StarEqualsToken,
    SlashEqualsToken,
    AmpersandEqualsToken,
    PipeEqualsToken,
    CircumflexEqualsToken,

    PlusPlusToken,
    MinusMinusToken,

    BangToken,
    AmpersandToken,
    AmpersandAmpersandToken,
    PipeToken,
    PipePipeToken,
    CircumflexToken,
    TildeToken,
    EqualsEqualsToken,
    BangEqualsToken,
    GreaterToken,
    GreaterOrEqualsToken,
    LessToken,
    LessOrEqualsToken,

    IdentifierToken,

    CommaToken,
    ColonToken,
    #endregion
    #region Keywords
    TrueKeyword,
    FalseKeyword,

    LetKeyword,
    VarKeyword,

    IfKeyword,
    ElseKeyword,

    WhileKeyword,
    DoKeyword,

    ForKeyword,
    ToKeyword,

    ContinueKeyword,
    BreakKeyword,

    FunctionKeyword,
    ReturnKeyword,
    #endregion
    #region Nodes
    CompilationUnit,
    Parameter,
    #endregion
    #region Expressions
    LiteralExpression,
    NameExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
    CallExpression,
    PrefixExpression,
    PostfixExpression,
    #endregion
    #region Statements
    BlockStatement,
    ExpressionStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
    ContinueStatement,
    BreakStatement,
    ReturnStatement,
    #endregion
    #region Clauses
    ElseClause,
    TypeClause,
    #endregion
    #region Members
    GlobalStatement,
    FunctionDeclaration
    #endregion
}
