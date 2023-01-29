using System.Collections.Immutable;
using Balu.Symbols;
using Balu.Syntax;
using Balu.Text;

namespace Balu.Binding;

static class BoundNodeFactory
{
    public static BoundAssignmentExpression Assignment(SyntaxNode syntax, VariableSymbol variable, BoundExpression expression) =>
        new(syntax, variable, expression);
    public static BoundBinaryExpression Binary(SyntaxNode syntax, BoundExpression left, BoundBinaryOperator op, BoundExpression right) => new(syntax, left, op, right);
    public static BoundBinaryExpression Add(SyntaxNode syntax, BoundExpression left, BoundExpression right)
        => Binary(syntax, left, BoundBinaryOperator.BinaryPlus, right);
    public static BoundBinaryExpression LessOrEqual(SyntaxNode syntax, BoundExpression left, BoundExpression right)
        => Binary(syntax, left, BoundBinaryOperator.LessOrEquals, right);
    public static BoundBlockStatement Block(SyntaxNode syntax, params BoundStatement[] statements) => new(syntax, statements.ToImmutableArray());
    public static BoundBlockStatement Block(SyntaxNode syntax, ImmutableArray<BoundStatement> statements) => new(syntax, statements);
    public static BoundCallExpression Call(SyntaxNode syntax, FunctionSymbol function, params BoundExpression[] arguments) => new(syntax, function, arguments.ToImmutableArray());
    public static BoundCallExpression Call(SyntaxNode syntax, FunctionSymbol function, ImmutableArray<BoundExpression> arguments) => new(syntax, function, arguments);
    public static BoundConversionExpression Conversion(SyntaxNode syntax, TypeSymbol type, BoundExpression expression) => new(syntax, type, expression);
    public static BoundDoWhileStatement DoWhile(SyntaxNode syntax, BoundStatement body, BoundExpression condition, BoundLabel breakLabel,
                                                BoundLabel continueLabel) => new(syntax, body, condition, breakLabel, continueLabel);
    public static BoundErrorExpression Error(SyntaxNode syntax) => new(syntax);
    public static BoundExpressionStatement Expression(SyntaxNode syntax, BoundExpression expression) => new(syntax, expression);
    public static BoundExpressionStatement Increment(SyntaxNode syntax, BoundVariableExpression variable)
    {
        var increment = Add(syntax, variable, Literal(syntax, 1));
        var incrementAssign = new BoundAssignmentExpression(syntax, variable.Variable, increment);
        return new (syntax, incrementAssign);
    }
    public static BoundForStatement For(SyntaxNode syntax, VariableSymbol variable, BoundExpression lowerBound, BoundExpression upperBound,
                                        BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) =>
        new(syntax, variable, lowerBound, upperBound, body, breakLabel, continueLabel);
    public static BoundGotoStatement Goto(SyntaxNode syntax, BoundLabel label) => new(syntax, label);
    public static BoundConditionalGotoStatement GotoTrue(SyntaxNode syntax, BoundLabel label, BoundExpression condition) =>
        new(syntax, label, condition);
    public static BoundConditionalGotoStatement GotoFalse(SyntaxNode syntax, BoundLabel label, BoundExpression condition) =>
        new(syntax, label, condition, false);
    public static BoundIfStatement If(SyntaxNode syntax, BoundExpression condition, BoundStatement thenStatement,
                                      BoundStatement? elseStatement = null) => new(syntax, condition, thenStatement, elseStatement);
    public static BoundLabelStatement Label(SyntaxNode syntax, BoundLabel label) => new (syntax, label);
    public static BoundLiteralExpression Literal(SyntaxNode syntax, object value) => new(syntax, value);
    public static BoundNopStatement Nop(SyntaxNode syntax) => new(syntax);
    public static BoundReturnStatement Return(SyntaxNode syntax, BoundExpression? expression = null) => new(syntax, expression);
    public static BoundSequencePointStatement SequencePoint(BoundStatement statement, TextLocation location) =>
        new (statement.Syntax, statement, location);
    public static BoundBeginScopeStatement BeginScope(SyntaxNode syntax) => new(syntax);
    public static BoundEndScopeStatement EndScope(SyntaxNode syntax) => new(syntax);
    public static BoundStatement UnwrapSequencePoint(this BoundStatement statement) =>
        statement.Kind == BoundNodeKind.SequencePointStatement ? ((BoundSequencePointStatement)statement).Statement : statement;
    public static BoundUnaryExpression Unary(SyntaxNode syntax, BoundUnaryOperator op, BoundExpression operand) => new(syntax, op, operand);
    public static BoundVariableDeclarationStatement VariableDeclaration(SyntaxNode syntax, VariableSymbol symbol, BoundExpression initializer) => new (syntax, symbol, initializer);
    public static BoundVariableDeclarationStatement VariableDeclaration(SyntaxNode syntax, string name, BoundExpression initializer) => VariableDeclaration(syntax, name, initializer, isReadOnly: false);
    public static BoundVariableDeclarationStatement ConstantDeclaration(SyntaxNode syntax, string name, BoundExpression initializer) => VariableDeclaration(syntax, name, initializer, isReadOnly: true);
    static BoundVariableDeclarationStatement VariableDeclaration(SyntaxNode syntax, string name, BoundExpression initializer, bool isReadOnly)
    {
        var local = new LocalVariableSymbol(name, isReadOnly, initializer.Type, initializer.Constant);
        return new(syntax, local, initializer);
    }
    public static BoundVariableExpression Variable(SyntaxNode syntax, VariableSymbol variable) => new(syntax, variable);
    public static BoundWhileStatement While(SyntaxNode syntax, BoundExpression condition, BoundStatement body, BoundLabel breakLabel,
                                            BoundLabel continueLabel) => new(syntax, condition, body, breakLabel, continueLabel);

    public static BoundPrefixExpression Prefix(SyntaxNode syntax, BoundBinaryOperator op, VariableSymbol variable) => new(syntax, op, variable);
    public static BoundPostfixExpression Postfix(SyntaxNode syntax, VariableSymbol variable, BoundBinaryOperator op) => new(syntax, variable, op);
}