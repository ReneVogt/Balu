namespace Balu.Syntax;

/// <summary>
/// The abstract base class for <see cref="SyntaxNode"/> visitors.
/// </summary>
public abstract class SyntaxVisitor
{
    /// <summary>
    /// Constructs a <see cref="SyntaxVisitor"/> instance.
    /// </summary>
    protected SyntaxVisitor(){}

    /// <summary>
    /// Visits a <see cref="SyntaxNode"/> by calling the correct method for the node's <see cref="SyntaxKind"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxNode"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    public virtual SyntaxNode Visit(SyntaxNode node)
    {
        return node switch
        {
            SyntaxToken token => VisitToken(token),
            CompilationUnitSyntax compilationUnit => VisitCompilationUnit(compilationUnit),
            LiteralExpressionSyntax literal => VisitLiteralExpression(literal),
            UnaryExpressionSyntax unary => VisitUnaryExpression(unary),
            BinaryExpressionSyntax binary => VisitBinaryExpression(binary),
            ParenthesizedExpressionSyntax parenthesized => VisitParenthesizedExpression(parenthesized),
            NameExpressionSyntax name => VisitNameExpression(name),
            AssignmentExpressionSyntax assignment => VisitAssignmentExpression(assignment),
            BlockStatementSyntax block => VisitBlockStatement(block),
            ExpressionStatementSyntax expression => VisitExpressionStatement(expression),
            VariableDeclarationStatementSyntax variable => VisitVariableDeclarationStatement(variable),
            IfStatementSyntax ifStatement => VisitIfStatement(ifStatement),
            ElseClauseSyntax elseClause => VisitElseClause(elseClause),
            WhileStatementSyntax whileStatement => VisitWhileStatement(whileStatement),
            _ => node.Accept(this)
        };
    }

    /// <summary>
    /// Visits a <see cref="SyntaxToken"/>.
    /// </summary>
    /// <param name="node">The <see cref="SyntaxToken"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxToken"/>.</returns>
    protected virtual SyntaxNode VisitToken(SyntaxToken node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="CompilationUnitSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="CompilationUnitSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="LiteralExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="LiteralExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="UnaryExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="UnaryExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="BinaryExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BinaryExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="ParenthesizedExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ParenthesizedExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="NameExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="NameExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitNameExpression(NameExpressionSyntax node) => node.Accept(this);

    /// <summary>
    /// Visits a <see cref="AssignmentExpressionSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="AssignmentExpressionSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="BlockStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="BlockStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitBlockStatement(BlockStatementSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="ExpressionStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ExpressionStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="VariableDeclarationStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="VariableDeclarationStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="IfStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="IfStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitIfStatement(IfStatementSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="ElseClauseSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="ElseClauseSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitElseClause(ElseClauseSyntax node) => node.Accept(this);
    /// <summary>
    /// Visits a <see cref="WhileStatementSyntax"/>.
    /// </summary>
    /// <param name="node">The <see cref="WhileStatementSyntax"/> to visit.</param>
    /// <returns>The original <paramref name="node"/> or a transformed <see cref="SyntaxNode"/>.</returns>
    protected virtual SyntaxNode VisitWhileStatement(WhileStatementSyntax node) => node.Accept(this);
}
