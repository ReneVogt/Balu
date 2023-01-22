using System;

namespace Balu.Symbols;

public static class GlobalSymbolNames
{
    public const string Main = "main";
    public const string Eval = "<eval>";
    public const string Random = "<random>";
    public const string Result = "<result>";

    public static bool IsBaluSpecialName(this string name) => name?.StartsWith("<", StringComparison.InvariantCulture) ?? throw new ArgumentNullException(nameof(name));
}