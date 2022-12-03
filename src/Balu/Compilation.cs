using System;
using System.IO;
using System.Linq;
using Balu.Binding;
using Balu.Evaluation;
using Balu.Syntax;
using Balu.Visualization;

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
    /// <param name="analyzisWriter">An optional <see cref="TextWriter"/> to write syntax and/or bound tree to.</param>
    /// <param name="showSyntaxTree">Indicates wether the syntax tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <param name="showBoundTree">Indicates wether the bound tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    public EvaluationResult Evaluate(TextWriter? analyzisWriter = null, bool showSyntaxTree = false, bool showBoundTree = false)
    {
        var boundTree = BoundTree.Bind(SyntaxTree);
        if (analyzisWriter is not null)
        {
            if (showSyntaxTree) SyntaxTreePrinter.Print(SyntaxTree.Root, analyzisWriter);
            if (showBoundTree) BoundTreePrinter.Print(boundTree.Root, analyzisWriter);
        }
        return boundTree.Diagnostics.Any()
                   ? new(boundTree.Diagnostics, null)
                   : new(Array.Empty<Diagnostic>(), Evaluator.Evaluate(boundTree.Root));
    }

    /// <summary>
    /// Evaluates the given Balu <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The string containing the Balu input code.</param>
    /// <param name="analyzisWriter">An optional <see cref="TextWriter"/> to write syntax and/or bound tree to.</param>
    /// <param name="showSyntaxTree">Indicates wether the syntax tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <param name="showBoundTree">Indicates wether the bound tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    public static EvaluationResult Evaluate(string input, TextWriter? analyzisWriter = null, bool showSyntaxTree = false, bool showBoundTree = false) => Evaluate(SyntaxTree.Parse(input ?? throw new ArgumentNullException(nameof(input))), analyzisWriter, showSyntaxTree, showBoundTree);

    /// <summary>
    /// Evaluates the given <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> to bind and evaluate.</param>
    /// <param name="analyzisWriter">An optional <see cref="TextWriter"/> to write syntax and/or bound tree to.</param>
    /// <param name="showSyntaxTree">Indicates wether the syntax tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <param name="showBoundTree">Indicates wether the bound tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    public static EvaluationResult Evaluate(SyntaxTree syntaxTree, TextWriter? analyzisWriter = null, bool showSyntaxTree = false, bool showBoundTree = false) => new Compilation(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))).Evaluate(analyzisWriter, showSyntaxTree, showBoundTree);
}
