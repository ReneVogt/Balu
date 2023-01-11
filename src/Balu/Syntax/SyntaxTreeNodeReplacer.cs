namespace Balu.Syntax;

sealed class SyntaxTreeNodeReplacer : SyntaxTreeRewriter
{
    readonly SyntaxNode toReplace;
    readonly SyntaxNode replacement;

    SyntaxTreeNodeReplacer(SyntaxNode toReplace, SyntaxNode replacement) => (this.toReplace, this.replacement) = (toReplace, replacement);
    public override SyntaxNode Visit(SyntaxNode node) => node == toReplace ? replacement : base.Visit(node);

    public static SyntaxNode Replace(SyntaxNode source, SyntaxNode toReplace, SyntaxNode replacement) =>
        new SyntaxTreeNodeReplacer(toReplace, replacement).Visit(source);
}