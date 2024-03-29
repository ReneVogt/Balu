﻿using System;
using System.Collections.Immutable;

namespace Balu.Syntax;

public sealed partial class BlockStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<StatementSyntax> Statements { get; }
    public SyntaxToken ClosedBraceToken { get; }

    internal BlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closedBraceToken)
    : base(syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree)))
    {
        OpenBraceToken = openBraceToken ?? throw new ArgumentNullException(nameof(openBraceToken));
        Statements = statements;
        ClosedBraceToken = closedBraceToken ?? throw new ArgumentNullException(nameof(closedBraceToken));
    }
}
