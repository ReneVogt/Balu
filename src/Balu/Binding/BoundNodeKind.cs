namespace Balu.Binding;

enum BoundNodeKind
{
    UnaryExpression,
    BinaryExpression,
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,

    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement
}
