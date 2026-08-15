using Balu.Diagnostics;
using Balu.Syntax;
using Balu.Text;
using Xunit;

namespace Balu.Tests.BinderTests;

public sealed partial class BinderTests
{
    [Fact]
    public void Binder_MissingTypeIdentifier_ReportsAtInsertionPoint()
    {
        var compilation = Compilation.CreateScript(null, SyntaxTree.Parse("var x: = 1"));

        Assert.Collection(
            compilation.Diagnostics,
            diagnostic =>
            {
                Assert.Equal(DiagnosticId.UnexpectedToken, diagnostic.Id);
                Assert.Equal(new TextSpan(7, 1), diagnostic.Location.Span);
            },
            diagnostic =>
            {
                Assert.Equal(DiagnosticId.UndefinedType, diagnostic.Id);
                Assert.Equal(new TextSpan(7, 0), diagnostic.Location.Span);
            });
    }
}
