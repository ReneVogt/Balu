using Balu.Symbols;
using Balu.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Balu.Binding;

sealed class BoundCallExpression : BoundExpression
{
    public override TypeSymbol Type => Function.ReturnType;

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

    public override IEnumerable<BoundNode> Children => Arguments;
    public override bool HasSideEffects => true;

    public FunctionSymbol Function { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    internal BoundCallExpression(SyntaxNode syntax, FunctionSymbol function, ImmutableArray<BoundExpression> arguments) : base(syntax)
    {
        Function = function;
        Arguments = arguments;
    }

    internal override BoundNode Rewrite(BoundTreeRewriter rewriter)
    {
        var arguments = RewriteList(rewriter, Arguments);
        return arguments == Arguments ? this : new(Syntax, Function, arguments);
    }

    public override string ToString() => $"{Function.Name}({string.Join(", ", Arguments)})";
}
