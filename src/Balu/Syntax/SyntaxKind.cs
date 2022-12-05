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
    /// An open parenthesis token.
    /// </summary>
    OpenParenthesisToken,

    /// <summary>
    /// A closed parenthesis token.
    /// </summary>
    ClosedParenthesisToken,

    /// <summary>
    /// An '=' token.
    /// </summary>
    EqualsToken,

    /// <summary>
    /// A '!' token.
    /// </summary>
    BangToken,

    /// <summary>
    /// A '&&' token.
    /// </summary>
    AmpersandAmpersandToken,

    /// <summary>
    /// A '||' token.
    /// </summary>
    PipePipeToken,

    /// <summary>
    /// A '==' token.
    /// </summary>
    EqualsEqualsToken,

    /// <summary>
    /// A '!=' token.
    /// </summary>
    BangEqualsToken,

    /// <summary>
    /// An arbitrary identifier token.
    /// </summary>
    IdentifierToken,
    #endregion
    #region Keywords
    /// <summary>
    /// The <c>"true</c> keyword.
    /// </summary>
    TrueKeyword,

    /// <summary>
    /// The <c>"false</c> keyword.
    /// </summary>
    FalseKeyword,
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
    AssignmentExpression
    #endregion
}
