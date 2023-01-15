using Balu.Symbols;
using Balu.Syntax;
using System.Collections.Immutable;

namespace Balu.Binding;

sealed partial class BoundCallExpression : BoundExpression
{
    public override TypeSymbol Type => Function.ReturnType;

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

    public override bool HasSideEffects => true;

    public FunctionSymbol Function { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    internal BoundCallExpression(SyntaxNode syntax, FunctionSymbol function, ImmutableArray<BoundExpression> arguments) : base(syntax)
    {
        Function = function;
        Arguments = arguments;
    }

    public override string ToString() => $"{Function.Name}({string.Join(", ", Arguments)})";
}
