using Balu.Symbols;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Binding;

sealed class BoundCallExpression : BoundExpression
{
    public override TypeSymbol Type => Function.ReturnType;

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

    public override IEnumerable<BoundNode> Children => Arguments;

    public FunctionSymbol Function { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    internal BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
    {
        Function = function;
        Arguments = arguments;
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        var arguments = VisitList(visitor, Arguments);
        return arguments == Arguments ? this : new(Function, arguments);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Function.Name}({string.Join(", ", Arguments)})";
}
