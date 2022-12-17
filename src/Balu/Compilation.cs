using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Balu.Binding;
using Balu.Evaluation;
using Balu.Lowering;
using Balu.Syntax;
using Balu.Visualization;
#pragma warning disable CA1724
namespace Balu;

/// <summary>
/// Represents a complete Balu compilation.
/// </summary>
public sealed class Compilation
{
    BoundGlobalScope? globalScope;
    BoundBlockStatement? loweredStatement;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (globalScope is null)
            {
                var scope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                Interlocked.CompareExchange(ref globalScope, scope, null);
            }

            return globalScope;
        }
    }
    internal BoundBlockStatement LoweredStatement
    {
        get
        {
            if (loweredStatement is null)
            {
                var statement = Lowerer.Lower(GlobalScope.Statement);
                Interlocked.CompareExchange(ref loweredStatement, statement, null);
            }

            return loweredStatement;
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
    /// <param name="globals">A <see cref="VariableDictionary"/> for storing globals and their values.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="globals"/> is <c>>null</c>.</exception>
    public EvaluationResult Evaluate(VariableDictionary globals)
    {
        _ = globals ?? throw new ArgumentNullException(nameof(globals));
        var diagnostics = SyntaxTree.Diagnostics.AddRange(GlobalScope.Diagnostics);
        if (diagnostics.Any())
            return new(diagnostics, null);

        var program = Binder.BindProgram(GlobalScope);
        if (program.Diagnostics.Any())
            return new(diagnostics, null);

        return new(ImmutableArray<Diagnostic>.Empty, Evaluator.Evaluate(LoweredStatement, globals, program.Functions));
    }

    /// <summary>
    /// Writes a text representation of the <see cref="SyntaxTree"/> to the provided <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
    public void WriteSyntaxTree(TextWriter writer) => SyntaxTreeWriter.Print(SyntaxTree.Root, writer ?? throw new ArgumentNullException(nameof(writer)));
    /// <summary>
    /// Writes a text representation of the bound program tree to the provided <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
    public void WriteBoundTree(TextWriter writer) =>
        BoundTreeWriter.Print(GlobalScope.Statement, writer ?? throw new ArgumentNullException(nameof(writer)));
    /// <summary>
    /// Writes a text representation of the lowered program tree to the provided <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <c>null</c>.</exception>
    public void WriteLoweredTree(TextWriter writer) =>
        BoundTreeWriter.Print(LoweredStatement, writer ?? throw new ArgumentNullException(nameof(writer)));
    /// <summary>
    /// Evaluates the given Balu <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The string containing the Balu input code.</param>
    /// <param name="globals">A <see cref="VariableDictionary"/> for storing globals and their values.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="globals"/> is <c>>null</c>.</exception>
    public static EvaluationResult Evaluate(string input, VariableDictionary globals) => Evaluate(SyntaxTree.Parse(input ?? throw new ArgumentNullException(nameof(input))), globals);

    /// <summary>
    /// Evaluates the given <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="syntaxTree">The <see cref="SyntaxTree"/> to bind and evaluate.</param>
    /// <param name="globals">A <see cref="VariableDictionary"/> for storing globals and their values.</param>
    /// <returns>An <see cref="EvaluationResult"/> containing the result of the evaluation or error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="syntaxTree"/> or <paramref name="globals"/> is <c>>null</c>.</exception>
    public static EvaluationResult Evaluate(SyntaxTree syntaxTree, VariableDictionary globals) => new Compilation(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree))).Evaluate(globals);
}
