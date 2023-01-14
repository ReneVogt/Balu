using Balu.Symbols;
using Balu.Syntax;

namespace Balu.Binding;

sealed partial class BoundLiteralExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override BoundConstant Constant { get; }

    public override TypeSymbol Type { get; }

    public object Value => Constant.Value;

    public BoundLiteralExpression(SyntaxNode syntax, object value) : base(syntax)
    {
        Constant = new (value);
        Type = Value switch
        {
            int => TypeSymbol.Integer,
            bool => TypeSymbol.Boolean,
            string => TypeSymbol.String,
            _ => throw new BindingException($"Invalid literal value type '{Value.GetType().Name}'.")
        };
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter) => this;

    public override string ToString() => Value.ToString() ?? string.Empty;
}