﻿using System.Collections.Generic;

namespace Balu.Syntax;

/// <summary>
/// Represents a 'while' statement.
/// </summary>
public sealed class WhileStatementSyntax : StatementSyntax
{
    /// <inheritdoc/>
    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
    /// <inheritdoc/>
    public override IEnumerable<SyntaxNode> Children
    {
        get
        {
            yield return WhileKeyword;
            yield return Condition;
            yield return Body;
        }
    }

    /// <summary>
    /// The <see cref="SyntaxToken"/> representing the 'while' keyword.
    /// </summary>
    public SyntaxToken WhileKeyword { get; }
    /// <summary>
    /// The 'while' condition.
    /// </summary>
    public ExpressionSyntax Condition { get; }
    /// <summary>
    /// The body of the 'while' statement.
    /// </summary>
    public StatementSyntax Body { get; }

    internal WhileStatementSyntax(SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body)
    {
        WhileKeyword = whileKeyword;
        Condition = condition;
        Body = body;
    }
    internal override SyntaxNode Accept(SyntaxVisitor visitor)
    {
        var whileKeyword = (SyntaxToken)visitor.Visit(WhileKeyword);
        var condition = (ExpressionSyntax)visitor.Visit(Condition);
        var statement = (StatementSyntax)visitor.Visit(Body);
        return whileKeyword == WhileKeyword && condition == Condition && statement == Body
                   ? this
                   : WhileStatement(whileKeyword, condition, statement);
    }
}