using System;
using System.IO;
using System.Linq;
using System.Threading;
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
    BoundGlobalScope? globalScope;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (globalScope is null)
            {
                var scope = Binder.BindGlobalScope(SyntaxTree.Root);
                Interlocked.CompareExchange(ref globalScope, scope, null);
            }

            return globalScope;
        }
    }

    /// <summary>
    /// The compilation's <see cref="SyntaxTree"/>.
    /// </summary>
    public SyntaxTree SyntaxTree { get; }
    /// <summary>
    /// The previous compilation step.
    /// </summary>
    public Compilation? Previous { get; }

    /// <summary>
    /// Creates a new <see cref="Compilation"/> instance holding the provided <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> representing the root of this compilation.</param>
    public Compilation(SyntaxTree syntaxTree) : this(null, syntaxTree){}

    Compilation(Compilation? previous, SyntaxTree syntaxTree) => (Previous, SyntaxTree) = (previous, syntaxTree);

    /// <summary>
    /// Creates another compilation that keeps the scope of the current one.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> to continue with.</param>
    /// <returns>A new <see cref="Compilation"/> that continues the current one.</returns>
    public Compilation ContinueWith(SyntaxTree syntaxTree) => new (this, syntaxTree);

    /// <summary>
    /// Evaluates the <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="variables">A <see cref="VariableDictionary"/> for storing variables and their values.</param>
    /// <param name="analyzisWriter">An optional <see cref="TextWriter"/> to write syntax and/or bound tree to.</param>
    /// <param name="showSyntaxTree">Indicates wether the syntax tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <param name="showBoundTree">Indicates wether the bound tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="variables"/> is <c>>null</c>.</exception>
    public EvaluationResult Evaluate(VariableDictionary variables, TextWriter? analyzisWriter = null, bool showSyntaxTree = false, bool showBoundTree = false)
    {
        _ = variables ?? throw new ArgumentNullException(nameof(variables));
        if (analyzisWriter is not null)
        {
            if (showSyntaxTree) SyntaxTreeWriter.Print(SyntaxTree.Root, analyzisWriter);
            if (showBoundTree) BoundTreeWriter.Print(GlobalScope.Expression, analyzisWriter);
        }

        var diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToArray();
        return diagnostics.Any()
                   ? new(diagnostics, null)
                   : new(Array.Empty<Diagnostic>(), Evaluator.Evaluate(GlobalScope.Expression, variables));
    }

    /// <summary>
    /// Evaluates the given Balu <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The string containing the Balu input code.</param>
    /// <param name="variables">A <see cref="VariableDictionary"/> for storing variables and their values.</param>
    /// <param name="analyzisWriter">An optional <see cref="TextWriter"/> to write syntax and/or bound tree to.</param>
    /// <param name="showSyntaxTree">Indicates wether the syntax tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <param name="showBoundTree">Indicates wether the bound tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="variables"/> is <c>>null</c>.</exception>
    public static EvaluationResult Evaluate(string input, VariableDictionary variables, TextWriter? analyzisWriter = null, bool showSyntaxTree = false, bool showBoundTree = false) => Evaluate(SyntaxTree.Parse(input ?? throw new ArgumentNullException(nameof(input))), variables, analyzisWriter, showSyntaxTree, showBoundTree);

    /// <summary>
    /// Evaluates the given <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> to bind and evaluate.</param>
    /// <param name="variables">A <see cref="VariableDictionary"/> for storing variables and their values.</param>
    /// <param name="analyzisWriter">An optional <see cref="TextWriter"/> to write syntax and/or bound tree to.</param>
    /// <param name="showSyntaxTree">Indicates wether the syntax tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <param name="showBoundTree">Indicates wether the bound tree should be written to the <paramref name="analyzisWriter"/>.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="variables"/> is <c>>null</c>.</exception>
    public static EvaluationResult Evaluate(SyntaxTree syntaxTree, VariableDictionary variables, TextWriter? analyzisWriter = null, bool showSyntaxTree = false, bool showBoundTree = false) => new Compilation(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))).Evaluate(variables, analyzisWriter, showSyntaxTree, showBoundTree);
}
