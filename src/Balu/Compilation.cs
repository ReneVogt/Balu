using System;
using System.Linq;
using Balu.Binding;
using Balu.Evaluation;
using Balu.Syntax;

namespace Balu;

/// <summary>
/// Represents a complete Balu compilation.
/// </summary>
public sealed class Compilation
{
    /// <summary>
    /// The compilation's <see cref="SyntaxTree"/>.
    /// </summary>
    public SyntaxTree SyntaxTree { get; }

    /// <summary>
    /// Creates a new <see cref="Compilation"/> instance holding the provided <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> representing the root of this compilation.</param>
    public Compilation(SyntaxTree syntaxTree) => SyntaxTree = syntaxTree;

    /// <summary>
    /// Evaluates the <see cref="SyntaxTree"/>.
    /// </summary>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    public EvaluationResult Evaluate()
    {
        var boundTree = BoundTree.Bind(SyntaxTree);
        return boundTree.Diagnostics.Any()
                   ? new(boundTree.Diagnostics, null)
                   : new(Array.Empty<string>(), Evaluator.Evaluate(boundTree.Root));
    }

    /// <summary>
    /// Evaluates the given Balu <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The string containing the Balu input code.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    public static EvaluationResult Evaluate(string input) => Evaluate(SyntaxTree.Parse(input ?? throw new ArgumentNullException(nameof(input))));

    /// <summary>
    /// Evaluates the given <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> to bind and evaluate.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    public static EvaluationResult Evaluate(SyntaxTree syntaxTree) => new Compilation(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))).Evaluate();
}
