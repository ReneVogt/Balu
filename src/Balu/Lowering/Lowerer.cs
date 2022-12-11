using Balu.Binding;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeVisitor
{
    protected override BoundNode VisitBoundForStatement(BoundForStatement forStatement)
    {
        /*
         * Source statement:
         *   for <var> = <lower> to <upper>
         *     <body>
         *
         * Target statement:
         *   {
         *     var <var> = <lower>
         *     while <var> <= <upper>
         *     {
         *       <body>
         *       <var> = <var> + 1
         *     }
         *   }
         */

        var variable = new BoundVariableExpression(forStatement.Variable);
        var declaration = new BoundVariableDeclarationStatement(forStatement.Variable, forStatement.LowerBound);

        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(
                forStatement.Variable,
                new BoundBinaryExpression(
                    variable,
                    BoundBinaryOperator.BinaryPlus,
                    new BoundLiteralExpression(1))));

        var whileCondition = new BoundBinaryExpression(
            variable,
            BoundBinaryOperator.LessOrEquals,
            forStatement.UpperBound);
        var whileBody = new BoundBlockStatement(forStatement.Body, increment);
        var whileStatement = new BoundWhileStatement(whileCondition, whileBody);

        var rewritten = new BoundBlockStatement(declaration, whileStatement);
        return Visit(rewritten);
    }

    public static BoundStatement Lower(BoundStatement statement) => (BoundStatement)new Lowerer().Visit(statement);
}
