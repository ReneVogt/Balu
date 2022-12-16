namespace Balu.Binding;

enum BoundNodeKind
{
    UnaryExpression,
    BinaryExpression,
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    CallExpression,
    ConversionExpression,
    ErrorExpression,

    BlockStatement,
    ExpressionStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
    GotoStatement,
    ConditionalGotoStatement,
    LabelStatement
}
