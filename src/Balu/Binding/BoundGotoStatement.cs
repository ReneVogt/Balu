﻿using System;
using System.Collections.Generic;

namespace Balu.Binding;

sealed class BoundGotoStatement : BoundStatement
{
    public LabelSymbol Label { get; }

    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
    public override IEnumerable<BoundNode> Children { get; } = Array.Empty<BoundNode>();

    public BoundGotoStatement(LabelSymbol label) => Label = label;

    internal override BoundNode Accept(BoundTreeVisitor visitor) => this;

    public override string ToString() => $"{Kind} => {Label}";
}