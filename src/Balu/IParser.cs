using Balu.Expressions;

namespace Balu;

/// <summary>
/// Represents a parser for the Balu language.
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parses the provided input into an <see cref="ExpressionSyntax"/>.
    /// </summary>
    /// <returns>The resulting <see cref="ExpressionSyntax"/> representing the input Balu code.</returns>
    ExpressionSyntax Parse();
}
