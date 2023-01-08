﻿using Balu.Symbols;
namespace Balu.Binding;

abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
    
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public virtual BoundConstant? Constant { get; }

    public override string ToString() => $"{Kind} ({Type})";
}