using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Balu.Sdk;

public sealed class BaluCompiler : ToolTask
{
    protected override string ToolName => "dotnet";

    [Required]
    public string DotNetPath { get; set; } = string.Empty;

    [Required]
    public string CompilerPath { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] SourceFiles { get; set; } = [];

    [Required]
    public ITaskItem[] ReferencedAssemblies { get; set; } = [];

    [Required]
    public string OutputPath { get; set; } = string.Empty;

    public string SymbolPath { get; set; } = string.Empty;

    public bool Debug { get; set; }

    protected override Encoding ResponseFileEncoding { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    protected override string GenerateFullPathToTool() => Path.GetFullPath(DotNetPath);

    protected override string GenerateCommandLineCommands()
    {
        var builder = new CommandLineBuilder();
        builder.AppendTextUnquoted("exec");
        builder.AppendFileNameIfNotNull(Path.GetFullPath(CompilerPath));

        return builder.ToString();
    }

    protected override string GenerateResponseFileCommands()
    {
        var builder = new CommandLineBuilder(quoteHyphensOnCommandLine: false, useNewLineSeparator: true);
        builder.AppendSwitchIfNotNull("/o ", OutputPath);

        if (!string.IsNullOrWhiteSpace(SymbolPath))
            builder.AppendSwitchIfNotNull("/s ", SymbolPath);
        builder.AppendSwitchIfNotNull("/debug ", Debug.ToString());

        foreach (var reference in ReferencedAssemblies)
            builder.AppendSwitchIfNotNull("/r ", reference.GetMetadata("FullPath"));

        foreach (var sourceFile in SourceFiles)
            builder.AppendFileNameIfNotNull(sourceFile.GetMetadata("FullPath"));

        return builder.ToString();
    }
}
