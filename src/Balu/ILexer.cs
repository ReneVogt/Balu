using System.Collections.Generic;

namespace Balu;

/// <summary>
/// Represents a lexer for the Balu language.
/// </summary>
public interface ILexer
{
    /// <summary>
    /// Enumerates the <see cref="SyntaxToken">syntax tokens</see> from the input Balu code.
    /// </summary>
    /// <returns>A sequence of <see cref="SyntaxToken">syntax tokens</see>.</returns>
    IEnumerable<SyntaxToken> GetTokens();
}
