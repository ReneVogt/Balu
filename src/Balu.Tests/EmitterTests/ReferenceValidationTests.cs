using System;
using System.IO;
using System.Linq;
using Balu.Diagnostics;
using Balu.Syntax;
using Mono.Cecil;
using TestHelpers;
using Xunit;

namespace Balu.Tests.EmitterTests;

public partial class EmitterTests
{
    [Fact]
    public void Emitter_RejectsRequiredMethodWithWrongReturnType() =>
        AssertInvalidConsoleWrite(method => method.ReturnType = method.Module.TypeSystem.String, DiagnosticId.RequiredMethodNotFound);

    [Fact]
    public void Emitter_RejectsRequiredMethodWithWrongInvocationShape() =>
        AssertInvalidConsoleWrite(method => method.IsStatic = false, DiagnosticId.RequiredMethodNotFound);

    [Fact]
    public void Emitter_RejectsStaticRequiredInstanceMethod() =>
        AssertInvalidRequiredMethod("System.Random", "Next", "System.Int32", method => method.IsStatic = true, DiagnosticId.RequiredMethodNotFound);

    [Fact]
    public void Emitter_RejectsNonPublicRequiredMethod() =>
        AssertInvalidConsoleWrite(method => method.IsPublic = false, DiagnosticId.RequiredMethodNotFound);

    [Fact]
    public void Emitter_RejectsAmbiguousRequiredMethod() =>
        AssertInvalidConsoleWrite(CloneMethod, DiagnosticId.RequiredMethodAmbiguous);

    static void AssertInvalidConsoleWrite(Action<MethodDefinition> modify, DiagnosticId expectedDiagnostic)
        => AssertInvalidRequiredMethod("System.Console", "Write", "System.Object", modify, expectedDiagnostic);

    static void AssertInvalidRequiredMethod(string typeName, string methodName, string parameterTypeName, Action<MethodDefinition> modify, DiagnosticId expectedDiagnostic)
    {
        var directory = Directory.CreateTempSubdirectory("BaluEmitterValidation-");
        try
        {
            var references = CopyReferences(directory);
            ModifyRequiredMethod(references, typeName, methodName, parameterTypeName, modify);
            var compilation = Compilation.Create(SyntaxTree.Parse("function main() {}"));
            using var output = new MemoryStream();

            var diagnostics = compilation.Emit("InvalidReferences", references, output, null);

            Assert.Contains(diagnostics, diagnostic => diagnostic.Id == expectedDiagnostic);
            Assert.Equal(0, output.Length);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    static void ModifyRequiredMethod(string[] references, string typeName, string methodName, string parameterTypeName, Action<MethodDefinition> modify)
    {
        foreach (var reference in references)
        {
            using var assembly = AssemblyDefinition.ReadAssembly(reference, new ReaderParameters { InMemory = true });
            var type = assembly.MainModule.GetType(typeName);
            if (type is null) continue;
            var method = type.Methods.Single(candidate =>
                candidate.Name == methodName &&
                candidate.Parameters.Count == 1 &&
                candidate.Parameters[0].ParameterType.FullName == parameterTypeName);
            modify(method);
            assembly.Write(reference);
            return;
        }
        throw new InvalidOperationException($"Could not find required type '{typeName}'.");
    }

    static void CloneMethod(MethodDefinition method)
    {
        var clone = new MethodDefinition(method.Name, method.Attributes, method.ReturnType)
        {
            CallingConvention = method.CallingConvention,
            ExplicitThis = method.ExplicitThis,
            HasThis = method.HasThis
        };
        foreach (var parameter in method.Parameters)
            clone.Parameters.Add(new(parameter.Name, parameter.Attributes, parameter.ParameterType));
        method.DeclaringType.Methods.Add(clone);
    }
}
