namespace Balu;
static class SyntaxFacts
{
    public static int BinaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken or
            SyntaxKind.MinusToken => 1,
        SyntaxKind.SlashToken or
            SyntaxKind.StarToken => 2,
        _ => 0
    };
}
