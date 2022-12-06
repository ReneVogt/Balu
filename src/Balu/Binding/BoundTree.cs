using System.Collections.Generic;
using System.Collections.Immutable;
using Balu.Syntax;

namespace Balu.Binding;

sealed class BoundTree
{
    public BoundExpression Root { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    
    public BoundTree(BoundExpression root, IEnumerable<Diagnostic> diagnostics)
    {
        Root = root;
        Diagnostics = diagnostics.ToImmutableArray();
    }

    public static BoundTree Bind(ExpressionSyntax syntax, VariableDictionary variables) => Binder.Bind(syntax, variables);
    public static BoundTree Bind(SyntaxTree syntax, VariableDictionary variables) => Binder.Bind(syntax, variables);
}
