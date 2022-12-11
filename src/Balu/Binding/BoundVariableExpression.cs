﻿using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundVariableExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override Type Type => Variable.Type;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public VariableSymbol Variable { get; }

    public BoundVariableExpression(VariableSymbol variable) => Variable = variable;

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;

    public override string ToString() => $"{Kind} \"{Variable.Name}\" ({Type.Name})";
}