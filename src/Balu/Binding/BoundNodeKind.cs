﻿namespace Balu.Binding;

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
    PostfixExpression,
    PrefixExpression,

    BlockStatement,
    ExpressionStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
    GotoStatement,
    ConditionalGotoStatement,
    LabelStatement,
    ReturnStatement,
    NopStatement,
    SequencePointStatement,
    BeginScopeStatement,
    EndScopeStatement
}