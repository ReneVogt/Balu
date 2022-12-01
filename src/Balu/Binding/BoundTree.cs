using System.Collections.Generic;
using System.Linq;
using Balu.Syntax;

namespace Balu.Binding;

sealed class BoundTree
{
    public BoundExpression Root { get; }
    public IReadOnlyList<string> Diagnostics { get; }
    
    public BoundTree(BoundExpression root, IEnumerable<string> diagnostics)
    {
        Root = root;
        Diagnostics = diagnostics.ToArray();
    }

    public static BoundTree Bind(ExpressionSyntax syntax) => Binder.Bind(syntax);
    public static BoundTree Bind(SyntaxTree syntax) => Binder.Bind(syntax);
}
