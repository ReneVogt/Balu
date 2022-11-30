using System;

namespace Balu.Binding;

abstract class BoundExpression : BoundNode
{
    public abstract Type Type { get; }
}
