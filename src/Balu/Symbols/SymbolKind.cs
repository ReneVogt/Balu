namespace Balu.Symbols;

/// <summary>
/// Kinds of symbols in the Balu language.
/// </summary>
public enum SymbolKind
{
    /// <summary>
    /// Global variable symbols.
    /// </summary>
    GlobalVariable,

    /// <summary>
    /// Local variable symbols.
    /// </summary>
    LocalVariable, 

    /// <summary>
    /// Function parameter symbols.
    /// </summary>
    Parameter,

    /// <summary>
    /// Type symbols.
    /// </summary>
    Type,

    /// <summary>
    /// Function symbols.
    /// </summary>
    Function
}
