using System.Collections.Generic;
using System.Linq;
using Balu.Expressions;

namespace Balu;

/// <summary>
/// A syntax tree representing a Balu code file.
/// </summary>
public sealed class SyntaxTree
{
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }
    public IReadOnlyCollection<string> Diagnostics { get; }

    internal SyntaxTree(ExpressionSyntax root, SyntaxToken endOfFileToken, IEnumerable<string> diagnostics) =>
        (Root, EndOfFileToken, Diagnostics) = (root, endOfFileToken, diagnostics.ToArray());

    public static SyntaxTree Parse(string input) => new Parser(input).Parse();
}
