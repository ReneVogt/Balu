﻿using System;
namespace Balu.Binding;

abstract class BoundExpression : BoundNode
{
    public abstract Type Type { get; }
    public override string ToString() => $"{Kind} ({Type})";
}
