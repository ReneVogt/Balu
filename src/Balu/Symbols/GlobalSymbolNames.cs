namespace Balu.Symbols;

static class GlobalSymbolNames
{
    public const string Main = "main";
    public const string Eval = "<eval>";
    public const string Random = "<random>";
    public const string Result = "<result>";

    public static bool IsSpecialName(this string name) => name.StartsWith('<');
}