namespace Balu.Syntax;

/// <summary>
/// The kind of a <see cref="SyntaxNode"/>.
/// </summary>
public enum SyntaxKind
{
    #region Tokens
    /// <summary>
    /// The token indicating the end of the input.
    /// </summary>
    EndOfFileToken,

    /// <summary>
    /// A sequence of whitespaces.
    /// </summary>
    WhiteSpaceToken,

    /// <summary>
    /// An invalid token in the source code.
    /// </summary>
    BadToken,

    /// <summary>
    /// A number (currently integer only).
    /// </summary>
    NumberToken,


    /// <summary>
    /// A string token, delimited by double-quotes.
    /// </summary>
    StringToken,

    /// <summary>
    /// The plus (+) token.
    /// </summary>
    PlusToken,

    /// <summary>
    /// The minus (-) token.
    /// </summary>
    MinusToken,

    /// <summary>
    /// The star (*) token.
    /// </summary>
    StarToken,

    /// <summary>
    /// The slash (/) token.
    /// </summary>
    SlashToken,

    /// <summary>
    /// An open parenthesis '(' token.
    /// </summary>
    OpenParenthesisToken,

    /// <summary>
    /// A closed parenthesis ')' token.
    /// </summary>
    ClosedParenthesisToken,

    /// <summary>
    /// An open brace '{' token.
    /// </summary>
    OpenBraceToken,

    /// <summary>
    /// A closed brace '}' token.
    /// </summary>
    ClosedBraceToken,

    /// <summary>
    /// An '=' token.
    /// </summary>
    EqualsToken,

    /// <summary>
    /// A '!' token.
    /// </summary>
    BangToken,

    /// <summary>
    /// A '&' token.
    /// </summary>
    AmpersandToken,
    /// <summary>
    /// A '&&' token.
    /// </summary>
    AmpersandAmpersandToken,

    /// <summary>
    /// A '|' token.
    /// </summary>
    PipeToken,
    /// <summary>
    /// A '||' token.
    /// </summary>
    PipePipeToken,

    /// <summary>
    /// A '^' token.
    /// </summary>
    CircumflexToken,

    /// <summary>
    /// A '~' token.
    /// </summary>
    TildeToken,

    /// <summary>
    /// A '==' token.
    /// </summary>
    EqualsEqualsToken,

    /// <summary>
    /// A '!=' token.
    /// </summary>
    BangEqualsToken,

    /// <summary>
    /// A '&gt;' token.
    /// </summary>
    GreaterToken,

    /// <summary>
    /// A '&gt;=' token.
    /// </summary>
    GreaterOrEqualsToken,

    /// <summary>
    /// A '&lt;' token.
    /// </summary>
    LessToken,

    /// <summary>
    /// A '&lt;=' token.
    /// </summary>
    LessOrEqualsToken,
    
    /// <summary>
    /// An arbitrary identifier token.
    /// </summary>
    IdentifierToken,

    /// <summary>
    /// A ',' token.
    /// </summary>
    CommaToken,
    #endregion
    #region Keywords
    /// <summary>
    /// The <c>true</c> keyword.
    /// </summary>
    TrueKeyword,

    /// <summary>
    /// The <c>false</c> keyword.
    /// </summary>
    FalseKeyword,

    /// <summary>
    /// The <c>let</c> keyword.
    /// </summary>
    LetKeyword,

    /// <summary>
    /// The <c>var</c> keyword.
    /// </summary>
    VarKeyword,

    /// <summary>
    /// The 'if' keyword.
    /// </summary>
    IfKeyword,
    /// <summary>
    /// The 'else' keyword.
    /// </summary>
    ElseKeyword,

    /// <summary>
    /// The 'while' keyword.
    /// </summary>
    WhileKeyword,

    /// <summary>
    /// The 'for' keyword.
    /// </summary>
    ForKeyword,
    /// <summary>
    /// The 'to' keyword.
    /// </summary>
    ToKeyword,
    #endregion
    #region Nodes
    CompilationUnit,
    #endregion
    #region Expressions
    /// <summary>
    /// A literal expression.
    /// </summary>
    LiteralExpression,

    /// <summary>
    /// A symbol name expression.
    /// </summary>
    NameExpression,

    /// <summary>
    /// A unary operator expression.
    /// </summary>
    UnaryExpression,

    /// <summary>
    /// A binary operator expression.
    /// </summary>
    BinaryExpression,

    /// <summary>
    /// A parenthesized expression.
    /// </summary>
    ParenthesizedExpression,

    /// <summary>
    /// An assignment expression (x = 42).
    /// </summary>
    AssignmentExpression,

    /// <summary>
    /// A function call expression.
    /// </summary>
    CallExpression,
    #endregion
    #region Statements
    /// <summary>
    /// A block statement surrounded by {}.
    /// </summary>
    BlockStatement,

    /// <summary>
    /// An expression statement.
    /// </summary>
    ExpressionStatement,

    /// <summary>
    /// A variable declaration.
    /// </summary>
    VariableDeclarationStatement,

    /// <summary>
    /// The 'if' statement.
    /// </summary>
    IfStatement,
    /// <summary>
    /// The 'else' clause.
    /// </summary>
    ElseClause,

    /// <summary>
    /// The 'while' statement.
    /// </summary>
    WhileStatement,

    /// <summary>
    /// The 'for' statement.
    /// </summary>
    ForStatement
    #endregion
}
