using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundIfStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

    public BoundExpression Condition { get; }
    public BoundStatement ThenStatement { get; }
    public BoundStatement? ElseStatement { get; }

    public BoundIfStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement thenStatement, BoundStatement? elseStatement)
        : base(syntax)
    {
        Condition = condition;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
    }
}