using Balu.Syntax;
using Balu.Text;

namespace Balu.Binding;

sealed partial class BoundSequencePointStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.SequencePointStatement;

    public BoundStatement Statement { get; }
    public TextLocation Location { get; }

    public BoundSequencePointStatement(SyntaxNode syntax, BoundStatement statement, TextLocation location)
        : base(syntax)
    {
        Statement = statement;
        Location = location;
    }
}