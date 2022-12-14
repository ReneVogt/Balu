using Balu.Symbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Balu.Binding;

sealed class BoundCallExpression : BoundExpression
{
    public override TypeSymbol Type => Function.ReturnType;

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

    public override IEnumerable<BoundNode> Children => Arguments;

    public FunctionSymbol Function { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    internal BoundCallExpression(FunctionSymbol function, IEnumerable<BoundExpression> arguments)
    {
        Function = function;
        Arguments = arguments.ToImmutableArray();
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor)
    {
        List<BoundExpression>? arguments = null;
        for (int i = 0; i < Arguments.Length; i++)
        {
            var expression = (BoundExpression)visitor.Visit(Arguments[i]);
            if (arguments is not null)
            {
                arguments.Add(expression);
                continue;
            }

            if (expression == Arguments[i]) continue;
            arguments = Arguments.Take(i+1).ToList();
        }

        return arguments is null ? this : new(Function, arguments);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Kind} ({Type}) {Function.Name}";
}
