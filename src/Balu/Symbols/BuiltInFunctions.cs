using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Balu.Symbols;

static class BuiltInFunctions
{
    public static FunctionSymbol Print { get; } =
        new FunctionSymbol("print", new[] { new ParameterSymbol("text", TypeSymbol.String) }, TypeSymbol.Void);
    public static FunctionSymbol Input { get; } =
        new FunctionSymbol("input", Array.Empty<ParameterSymbol>(), TypeSymbol.String);
    public static FunctionSymbol Random { get; } =
        new FunctionSymbol("random", new []{new ParameterSymbol("maximum", TypeSymbol.Integer)}, TypeSymbol.Integer);

    public static IEnumerable<FunctionSymbol> GetBuiltInFunctions() => typeof(BuiltInFunctions)
                                                                       .GetProperties(BindingFlags.Public | BindingFlags.Static)
                                                                       .Where(property => property.PropertyType == typeof(FunctionSymbol))
                                                                       .Select(property => (FunctionSymbol)property.GetValue(0)!);
}
