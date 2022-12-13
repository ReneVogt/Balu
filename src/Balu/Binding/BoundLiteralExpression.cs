using Balu.Symbols;
using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundLiteralExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public override TypeSymbol Type { get; }

    public object Value { get; }

    public BoundLiteralExpression(object value)
    {
        Value = value;
        Type = Value switch
        {
            int => TypeSymbol.Integer,
            bool => TypeSymbol.Boolean,
            string => TypeSymbol.String,
            _ => throw new BindingException($"Invalid literal value type '{Value.GetType().Name}'.")
        };
    }

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;

    public override string ToString() => $"{Kind} ({Type.Name}) {Value}";
}