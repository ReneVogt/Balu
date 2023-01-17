using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Balu.Sdk;

public sealed class BaluCompiler : ToolTask
{
    protected override string ToolName => "bc";

    [Required]
    public string BcPath { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] SourceFiles { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public ITaskItem[] ReferencedAssemblies { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string OutputPath { get; set; } = string.Empty;

    public string SymbolPath { get; set; } = string.Empty;

    public bool Debug { get; set; }

    protected override string GenerateFullPathToTool() => Path.GetFullPath(BcPath);
    protected override string GenerateCommandLineCommands() =>
        $"/o \"{OutputPath}\" {(string.IsNullOrWhiteSpace(SymbolPath) ? "" : $"/s \"{SymbolPath}\"")} {string.Join(" ", ReferencedAssemblies.Select(item => $"/r \"{item.GetMetadata("FullPath")}\""))} {string.Join(" ", SourceFiles.Select(item => $"\"{item.GetMetadata("FullPath")}\""))}";
}