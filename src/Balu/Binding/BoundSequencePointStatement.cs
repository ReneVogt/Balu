using Balu.Syntax;
using Balu.Text;

namespace Balu.Binding;

sealed partial class BoundSequencePointStatement(SyntaxNode syntax, BoundStatement statement, TextLocation location) : BoundStatement(syntax)
{
    public override BoundNodeKind Kind => BoundNodeKind.SequencePointStatement;

    public BoundStatement Statement { get; } = statement;
    public TextLocation Location { get; } = location;
}