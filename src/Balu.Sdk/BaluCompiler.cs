using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Balu.Sdk;

public sealed class BaluCompiler : Task
{
    [Required]
    public string CompilerPath { get; set; } = string.Empty;
    [Required]
    public ITaskItem[] SourceFiles { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public ITaskItem[] ReferencedAssemblies { get; set; } = Array.Empty<ITaskItem>();
    [Required]
    public string OutputPath { get; set; } = string.Empty;
    [Required]
    public bool Debug { get; set; }

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.High, $"CompilerPath {CompilerPath}");
        Log.LogMessage(MessageImportance.High, $"SourceFiles: {string.Join(", ", SourceFiles.Select(sf => sf.ItemSpec))}");
        Log.LogMessage(MessageImportance.High, $"RefAsm: {string.Join(", ", ReferencedAssemblies.Select(sf => sf.ItemSpec))}");
        Log.LogMessage(MessageImportance.High, $"OutputPath: {OutputPath}");
        return true;
    }
}