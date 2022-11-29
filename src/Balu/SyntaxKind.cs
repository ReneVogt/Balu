namespace Balu;

/// <summary>
/// The kind of a <see cref="SyntaxNode"/>.
/// </summary>
public enum SyntaxKind
{
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
    ClosedParenthesisToken
}
