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
public sealed class BoundNodeChildrenGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }
    public void Execute(GeneratorExecutionContext context)
    {
        using var source = new StringWriter();
        using var writer = new IndentedTextWriter(source, "    ");

        var compilation = (CSharpCompilation)context.Compilation;
        var boundNodeType = compilation.GetTypeByMetadataName("Balu.Binding.BoundNode");
        var immutableArrayType = compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");
        if (boundNodeType is null || immutableArrayType is null)
            return;

        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("using System.Collections.Immutable;");

        var types = compilation.Assembly.GetAllTypes();
        var boundNodeTypes = types.Where(t => !t.IsAbstract && t.IsPartial() && t.IsDerivedFrom(boundNodeType));

        foreach (var type in boundNodeTypes)
        {
            writer.WriteLine($"namespace {type.ContainingNamespace.GetFullName()}");
            writer.WriteLine("{");
            writer.Indent++;
            WriteType(writer, type, boundNodeType, immutableArrayType);
            writer.Indent--;
            writer.WriteLine("}");
        }

        context.AddSource(
            "BoundNodeChildren.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
        File.WriteAllText(@"C:\Privat\Sourcen\Balu\BoundNodeChildren.generated", source.ToString());
    }
    static void WriteType(IndentedTextWriter writer, INamedTypeSymbol type, INamedTypeSymbol boundNodeType, INamedTypeSymbol immutableArrayType)
    {
        writer.WriteLine($"partial class {type.Name}");
        writer.WriteLine("{");
        writer.Indent++;
        WriteChildrenCount(writer, type, boundNodeType, immutableArrayType);
        WriteGetChild(writer, type, boundNodeType, immutableArrayType);
        writer.Indent--;
        writer.WriteLine("}");
    }
    static void WriteChildrenCount(IndentedTextWriter writer, INamedTypeSymbol type, INamedTypeSymbol boundNodeType, INamedTypeSymbol immutableArrayType)
    {
        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                propertyType.IsDerivedFrom(boundNodeType))
                             .ToImmutableArray();
        var nonNullableProperties = properties.Where(property => property.NullableAnnotation != NullableAnnotation.Annotated).ToImmutableArray();
        var nullableProperties = properties.Where(property => property.NullableAnnotation == NullableAnnotation.Annotated).ToImmutableArray();
        var collections = type.GetMembers()
                              .OfType<IPropertySymbol>()
                              .Where(property => property.Type is INamedTypeSymbol propertyType && propertyType.IsGenericOfOf(immutableArrayType, boundNodeType))
                              .ToImmutableArray();

        if (nullableProperties.Length == 0)
        {
            writer.Write("public override int ChildrenCount => ");
            WriteNonNullableSum();
            writer.WriteLine(";");
            return;
        }

        if (nonNullableProperties.Length + collections.Length == 0 && nullableProperties.Length == 1)
        {
            writer.WriteLine($"public override int ChildrenCount => {nullableProperties[0].Name} is null ? 0 : 1;");
            return;
        }

        writer.WriteLine("public override int ChildrenCount");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("get");
        writer.WriteLine("{");
        writer.Indent++;
        writer.Write("int count = ");
        WriteNonNullableSum();
        writer.WriteLine(";");
        writer.WriteLine(string.Join(Environment.NewLine, nullableProperties.Select(property => $"if ({property.Name} is not null) count++;")));
        writer.WriteLine("return count;");
        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");

        void WriteNonNullableSum()
        {
            if (nonNullableProperties.Length > 0)
            {
                writer.Write(nonNullableProperties.Length);
                if (collections.Length > 0) writer.Write(" + ");
            }
            else if (collections.Length == 0) writer.Write("0");
            writer.Write(string.Join(" + ", collections.Select(collection => $"{collection.Name}.Length")));
        }
    }
    static void WriteGetChild(IndentedTextWriter writer, INamedTypeSymbol type, INamedTypeSymbol boundNodeType, INamedTypeSymbol immutableArrayType)
    {
        const string signature = "public override Balu.Binding.BoundNode GetChild(int index)";
        const string exception = "throw new ArgumentOutOfRangeException(\"index\")";

        var properties = type.GetMembers()
                             .OfType<IPropertySymbol>()
                             .Where(property => property.Type is INamedTypeSymbol propertyType &&
                                                (propertyType.IsDerivedFrom(boundNodeType) ||
                                                 propertyType.IsGenericOfOf(immutableArrayType, boundNodeType))).ToImmutableArray();

        if (properties.Length == 0)
        {
            writer.WriteLine($"{signature} => {exception};");
            return;
        }

        if (properties.Length == 1)
        {
            var property = properties[0];
            if (((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType))
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
        
        writer.WriteLine(signature);
        writer.WriteLine("{");
        writer.Indent++;

        int propIndex = 0;
        var leadingNonNullableProperties = properties
                                           .TakeWhile(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType) &&
                                                                  property.NullableAnnotation != NullableAnnotation.Annotated)
                                           .ToImmutableArray();
        if (properties.Any(property => ((INamedTypeSymbol)property.Type).IsDerivedFrom(boundNodeType) && property.NullableAnnotation == NullableAnnotation.Annotated))
            writer.WriteLine($"if (index < 0) {exception};");

        if (leadingNonNullableProperties.Length == 1)
            writer.WriteLine($"if (index == 0) return {leadingNonNullableProperties[0].Name};");
        else if (leadingNonNullableProperties.Length > 1)
        {
            writer.WriteLine("switch(index)");
            writer.WriteLine("{");
            writer.Indent++;
            foreach(var property in leadingNonNullableProperties)
                writer.WriteLine($"case {propIndex++}: return {property.Name};");
            writer.Indent--;
            writer.WriteLine("}");
        }
        
        if (leadingNonNullableProperties.Length < properties.Length)
            writer.WriteLine($"int adjustedIndex = index, propIndex = {propIndex};");
        for (int i = leadingNonNullableProperties.Length; i < properties.Length; i++)
        {
            var property = properties[i];
            var propertyType = (INamedTypeSymbol)property.Type;
            if (propertyType.IsDerivedFrom(boundNodeType))
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
            else
            {
                writer.WriteLine($"if (adjustedIndex - propIndex < {property.Name}.Length) return {property.Name}[adjustedIndex-propIndex];");
                if (i < properties.Length-1) writer.WriteLine($"propIndex += {property.Name}.Length;");
            }
        }

        writer.WriteLine($"{exception};");
        writer.Indent--;
        writer.WriteLine("}");
    }
}