using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Balu.SourceGenerator;

[Generator]
public sealed class SyntaxNodeChildrenGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }
    public void Execute(GeneratorExecutionContext context)
    {
        using var source = new StringWriter();
        using var writer = new IndentedTextWriter(source, "    ");

        var compilation = (CSharpCompilation)context.Compilation;
        var syntaxNodeType = compilation.GetTypeByMetadataName("Balu.Syntax.SyntaxNode");
        var separatedListType = compilation.GetTypeByMetadataName("Balu.Syntax.SeparatedSyntaxList`1");
        var immutableArrayType = compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");

        if (syntaxNodeType is null || separatedListType is null || immutableArrayType is null)
            return;

        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("using System.Collections.Immutable;");
        writer.WriteLine();

        var types = compilation.Assembly.GetAllTypes();
        var syntaxNodeTypes = types.Where(t => !t.IsAbstract && t.IsPartial() && t.IsDerivedFrom(syntaxNodeType));

        foreach (var type in syntaxNodeTypes)
        {
            using(new CurlyIndenter(writer, $"namespace {type.ContainingNamespace.GetFullName()}"))
            {
                WriteType(writer, type, syntaxNodeType, separatedListType, immutableArrayType);
            }
        }

        context.AddSource(
            "SyntaxNodeChildren.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
    }
    static void WriteType(IndentedTextWriter writer, INamedTypeSymbol type, INamedTypeSymbol syntaxNodeType, INamedTypeSymbol separatedListType, INamedTypeSymbol immutableArrayType)
    {
        using(new CurlyIndenter(writer, $"partial class {type.Name}"))
        {
            WriteChildrenCount(writer, type, syntaxNodeType, separatedListType, immutableArrayType);
            WriteGetChild(writer, type, syntaxNodeType, separatedListType, immutableArrayType);
        }
    }
    static void WriteChildrenCount(IndentedTextWriter writer, INamedTypeSymbol type, INamedTypeSymbol syntaxNodeType, INamedTypeSymbol separatedListType, INamedTypeSymbol immutableArrayType)
    {
        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                propertyType.IsDerivedFrom(syntaxNodeType))
                             .ToImmutableArray();
        var nonNullableProperties = properties.Where(property => property.NullableAnnotation != NullableAnnotation.Annotated).ToImmutableArray();
        var nullableProperties = properties.Where(property => property.NullableAnnotation == NullableAnnotation.Annotated).ToImmutableArray();
        var separatedLists = type.GetMembers()
                                 .OfType<IPropertySymbol>()
                                 .Where(property => property.Type is INamedTypeSymbol propertyType && propertyType.IsGenericListOf(separatedListType, syntaxNodeType))
                                 .ToImmutableArray();
        var immutableArrays = type.GetMembers()
                                 .OfType<IPropertySymbol>()
                                 .Where(property => property.Type is INamedTypeSymbol propertyType && propertyType.IsGenericListOf(immutableArrayType, syntaxNodeType))
                                 .ToImmutableArray();

        if (nullableProperties.Length == 0)
        {
            writer.Write("public override int ChildrenCount => ");
            WriteNonNullableSum();
            writer.WriteLine(";");
            return;
        }

        if (nonNullableProperties.Length + separatedLists.Length + immutableArrays.Length == 0 && nullableProperties.Length == 1)
        {
            writer.WriteLine($"public override int ChildrenCount => {nullableProperties[0].Name} is null ? 0 : 1;");
            return;
        }

        using(new CurlyIndenter(writer, "public override int ChildrenCount"))
        {
            using(new CurlyIndenter(writer, "get"))
            {
                writer.Write("int count = ");
                WriteNonNullableSum();
                writer.WriteLine(";");
                writer.WriteLine(
                    string.Join(Environment.NewLine, nullableProperties.Select(property => $"if ({property.Name} is not null) count++;")));
                writer.WriteLine("return count;");
            }
        }

        void WriteNonNullableSum()
        {
            StringBuilder builder = new();
            if (nonNullableProperties.Length > 0)
                builder.Append(nonNullableProperties.Length);
            if (immutableArrays.Length > 0)
            {
                if (builder.Length > 0) builder.Append(" + ");
                builder.Append(string.Join(" + ", immutableArrays.Select(prop => $"{prop.Name}.Length")));
            }
            if (separatedLists.Length > 0)
            {
                if (builder.Length > 0) builder.Append(" + ");
                builder.Append(string.Join(" + ", separatedLists.Select(prop => $"{prop.Name}.ElementsWithSeparators.Length")));
            }

            if (builder.Length == 0) builder.Append('0');
            writer.Write(builder.ToString());
        }
    }
    static void WriteGetChild(IndentedTextWriter writer, INamedTypeSymbol type, INamedTypeSymbol syntaxNodeType, INamedTypeSymbol separatedListType, INamedTypeSymbol immutableArrayType)
    {
        const string signature = "public override Balu.Syntax.SyntaxNode GetChild(int index)";
        const string exception = "throw new ArgumentOutOfRangeException(\"index\")";

        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                (propertyType.IsDerivedFrom(syntaxNodeType) ||
                                                 propertyType.IsGenericListOf(separatedListType, syntaxNodeType) ||
                                                 propertyType.IsGenericListOf(immutableArrayType, syntaxNodeType))).ToImmutableArray();

        if (properties.Length == 0)
        {
            writer.WriteLine($"{signature} => {exception};");
            return;
        }

        if (properties.Length == 1)
        {
            var property = properties[0];
            if (((INamedTypeSymbol)property.Type).IsDerivedFrom(syntaxNodeType))
            {
                writer.Write(signature + " => ");
                if (property.NullableAnnotation == NullableAnnotation.Annotated)
                    writer.Write($"{property.Name} is not null && ");
                writer.WriteLine($"index == 0 ? {property.Name} : {exception};");
            }
            else
                writer.WriteLine($"{signature} => {property.Name}[index];");

            return;
        }

        using(new CurlyIndenter(writer, signature))
        {
            int propIndex = 0;
            var leadingNonNullableProperties = properties
                                               .TakeWhile(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(syntaxNodeType) &&
                                                                      property.NullableAnnotation != NullableAnnotation.Annotated)
                                               .ToImmutableArray();
            if (properties.Any(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(syntaxNodeType) &&
                                           property.NullableAnnotation == NullableAnnotation.Annotated))
                writer.WriteLine($"if (index < 0) {exception};");

            if (leadingNonNullableProperties.Length == 1)
            {
                writer.WriteLine($"if (index == 0) return {leadingNonNullableProperties[0].Name};");
                propIndex = 1;
            }
            else if (leadingNonNullableProperties.Length > 1)
            {
                using(new CurlyIndenter(writer, "switch(index)"))
                {
                    foreach (var property in leadingNonNullableProperties)
                        writer.WriteLine($"case {propIndex++}: return {property.Name};");
                }
            }

            if (leadingNonNullableProperties.Length < properties.Length)
                writer.WriteLine($"int adjustedIndex = index, propIndex = {propIndex};");
            for (int i = leadingNonNullableProperties.Length; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyType = (INamedTypeSymbol)property.Type;
                if (propertyType.IsDerivedFrom(syntaxNodeType))
                {
                    if (propertyType.NullableAnnotation == NullableAnnotation.Annotated)
                    {
                        writer.WriteLine($"if ({property.Name} is null) adjustedIndex++;");
                        writer.WriteLine($"else if (adjustedIndex == propIndex) return {property.Name};");
                    }
                    else
                        writer.WriteLine($"if (adjustedIndex == propIndex) return {property.Name};");

                    if (i < properties.Length - 1) writer.WriteLine("propIndex++;");
                }
                else if (propertyType.IsGenericListOf(immutableArrayType, syntaxNodeType))
                {
                    writer.WriteLine($"if (adjustedIndex - propIndex < {property.Name}.Length) return {property.Name}[adjustedIndex-propIndex];");
                    if (i < properties.Length - 1) writer.WriteLine($"propIndex += {property.Name}.Length;");
                }
                else
                {
                    writer.WriteLine($"if (adjustedIndex - propIndex < {property.Name}.ElementsWithSeparators.Length) return {property.Name}.ElementsWithSeparators[adjustedIndex-propIndex];");
                    if (i < properties.Length - 1) writer.WriteLine($"propIndex += {property.Name}.ElementsWithSeparators.Length;");
                }
            }

            writer.WriteLine($"{exception};");
        }
    }
}