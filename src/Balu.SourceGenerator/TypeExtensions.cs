
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Balu.SourceGenerator;

static class TypeExtensions
{
    public static ImmutableArray<INamedTypeSymbol> GetAllTypes(this IAssemblySymbol assembly)
    {
        var result = new List<INamedTypeSymbol>();
        GetAllTypes(result, assembly.GlobalNamespace);
        return result.OrderBy(type => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToImmutableArray();
    }
    static void GetAllTypes(List<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol type)
            result.Add(type);

        foreach (var child in symbol.GetMembers().OfType<INamespaceOrTypeSymbol>())
            GetAllTypes(result, child);
    }
    public static bool IsDerivedFrom(this ITypeSymbol type, INamedTypeSymbol baseType)
    {
        var current = type;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;

            current = current.BaseType;
        }
        return false;
    }
    public static bool IsPartial(this INamedTypeSymbol type) => type.DeclaringSyntaxReferences.Select(declaration => declaration.GetSyntax())
                                                        .OfType<TypeDeclarationSyntax>()
                                                        .Any(syntax => syntax.Modifiers.Any(modifier => modifier.ValueText == "partial"));
    public static bool IsSupportedNodeType(this INamedTypeSymbol type) =>
        type.ContainingType is null && type.Arity == 0 && !type.IsRecord &&
        !type.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
             .OfType<TypeDeclarationSyntax>()
             .Any(syntax => syntax.Modifiers.Any(modifier => modifier.ValueText == "file"));
    public static string ToIdentifier(this string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
            ? $"@{name}"
            : name;
    public static string ToIdentifier(this ISymbol symbol) => symbol.Name.ToIdentifier();
    public static string ToFullyQualifiedName(this ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    public static bool IsGenericListOf(this INamedTypeSymbol type, INamedTypeSymbol listType, INamedTypeSymbol elementBaseType) =>
        type.TypeArguments.Length == 1 && type.TypeArguments[0].IsDerivedFrom(elementBaseType) &&
        SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, listType);
    public static string GetFullName(this INamespaceSymbol nameSpace)
    {
        var builder = new StringBuilder();
        var current = nameSpace;
        while (!string.IsNullOrWhiteSpace(current.Name))
        {
            if (builder.Length == 0)
                builder.Append(current.Name.ToIdentifier());
            else
                builder.Insert(0, $"{current.Name.ToIdentifier()}.");
            current = current.ContainingNamespace;
        }
        return builder.ToString();
    }
}
