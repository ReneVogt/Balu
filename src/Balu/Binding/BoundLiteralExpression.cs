using Balu.Symbols;
using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundLiteralExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();
    public override BoundConstant Constant { get; }

    public override TypeSymbol Type { get; }

    public object Value => Constant.Value;

    public BoundLiteralExpression(object value)
    {
        Constant = new (value);
        Type = Value switch
        {
            int => TypeSymbol.Integer,
            bool => TypeSymbol.Boolean,
            string => TypeSymbol.String,
            _ => throw new BindingException($"Invalid literal value type '{Value.GetType().Name}'.")
        };
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => Value.ToString() ?? string.Empty;
}