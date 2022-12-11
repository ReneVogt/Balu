using Balu.Binding;

namespace Balu.Lowering;

sealed class Lowerer : BoundTreeVisitor
{
    public static BoundStatement Lower(BoundStatement statement) => (BoundStatement)new Lowerer().Visit(statement);
}
