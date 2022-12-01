namespace Balu.Syntax;
static class SyntaxFacts
{
    public static int UnaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken => 100,
        _ => 0
    };
    public static int BinaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken => 1,
        SyntaxKind.SlashToken or
            SyntaxKind.StarToken => 2,
        _ => 0
    };
    public static SyntaxKind KeywordKind(this string literal) => literal switch
    {
        "true" => SyntaxKind.TrueKeyword,
        "false" => SyntaxKind.FalseKeyword,
        _ => SyntaxKind.IdentifierToken
    };
}
