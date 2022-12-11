using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundLabelStatement : BoundStatement
{
    public LabelSymbol Label { get; }

    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public BoundLabelStatement(LabelSymbol label) => Label = label;

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;

    public override string ToString() => $"{Kind} {Label.Name}";
}
