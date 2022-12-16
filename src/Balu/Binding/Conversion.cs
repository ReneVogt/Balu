using Balu.Symbols;

namespace Balu.Binding;

sealed class Conversion
{
    public static Conversion None { get; } = new(false, false, false);
    public static Conversion Identity { get; } = new(true, true, false);
    public static Conversion Implicit { get; } = new(true, false, true);
    public static Conversion Explicit { get; } = new(true, false, false);

    public bool Exists { get; }
    public bool IsIdentity { get; }
    public bool IsImplicit { get; }
    public bool IsExplicit => Exists && !IsImplicit;

    Conversion(bool exists, bool isIdentity, bool isImplicit)
    {
        Exists = exists;
        IsIdentity = isIdentity;
        IsImplicit = isImplicit;
    }

    public static Conversion Classify(TypeSymbol from, TypeSymbol to)
    {
        if (from == to) return Identity;
        if ((from == TypeSymbol.Boolean || from == TypeSymbol.Integer) && to == TypeSymbol.String)
            return Explicit;
        if (from == TypeSymbol.String && (to == TypeSymbol.Boolean || to == TypeSymbol.Integer))
            return Explicit;
        return Explicit;
    }
}
