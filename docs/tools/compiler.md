# Compiler command line

`bc` is the Balu command-line compiler. It parses one or more source files,
creates a normal Balu compilation, and emits a managed .NET assembly. It can
also emit a portable PDB.

`bc` is a low-level compiler interface. It does not create a complete modern
.NET application layout: there is no app host, `.runtimeconfig.json`,
`.deps.json`, or publish directory. `Balu.Sdk` integrates the same compiler into
the regular .NET build pipeline when a complete application layout is needed.

## Invocation

When working from the repository, run:

```console
dotnet run --project src/bc -- <source-paths> [options]
```

When using a built `bc.dll` from its complete build or package output directory,
run:

```console
dotnet exec path/to/bc.dll <source-paths> [options]
```

Keep `bc.runtimeconfig.json`, `bc.deps.json`, and the compiler dependencies next
to the DLL. This invocation also requires the .NET 10 runtime.

If `bc` is available as an executable on `PATH`, the equivalent form is:

```console
bc <source-paths> [options]
```

Display the built-in help:

```console
dotnet run --project src/bc -- --help
```

At least one source path is required unless help is requested. Source paths are
positional arguments. `bc` does not require the conventional `.b` extension.

## Options

| Option | Value | Description |
| --- | --- | --- |
| `-r=PATH`, `/r PATH` | Assembly path | Add a .NET reference assembly; repeat for every reference |
| `-o=PATH`, `/o PATH` | Assembly path | Set the emitted assembly path |
| `-s=PATH`, `/s PATH` | PDB path | Emit a portable PDB at this path |
| `-debug=BOOL`, `/debug BOOL` | Boolean | Control debugger-friendly IL when a PDB is emitted |
| `-m=NAME`, `/m NAME` | Name | Set the emitted assembly/module name |
| `-q`, `/q` | none | Suppress informational output and diagnostics |
| `-?`, `-h`, `--help`, `/help` | none | Display help and exit |

Mono.Options accepts both dash-style and slash-style options. The repository's
MSBuild task uses slash-style options, while the development launch profile
uses dash-style options with `=`.

### References: `-r`

`bc` does not add default runtime references. Every required reference assembly
must be supplied explicitly, using one `-r` or `/r` option per file.

The emitter resolves these runtime types and their required members:

```text
System.Object
System.Console
System.String
System.Convert
System.Int32
System.Boolean
System.Void
System.Random
System.Diagnostics.DebuggableAttribute
```

For a modern .NET reference pack, Balu normally needs:

```text
System.Runtime.dll
System.Runtime.Extensions.dll
System.Console.dll
```

All references must belong to a compatible target framework. Do not mix .NET
Framework and modern .NET reference assemblies.

On a machine with the .NET 10 SDK, the modern reference assemblies are normally
below a directory of this form:

```text
<dotnet-root>/packs/Microsoft.NETCore.App.Ref/<version>/ref/net10.0/
```

On Windows, a .NET Framework reference assembly such as the following can
provide the required framework surface in one file:

```text
C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll
```

References are used to resolve the runtime types and methods needed by emitted
Balu code. They do not expose arbitrary .NET APIs to Balu source code; Balu has
no general .NET interop syntax.

### Assembly output: `-o`

`-o` selects the assembly output path. If omitted, the first source path is
changed to use the `.dll` extension:

```text
sources/program.b -> sources/program.dll
```

The output path is converted to an absolute path. The target directory must
already exist.

The emitter creates a managed DLL module and assigns the Balu entry point to the
assembly. Changing the output extension does not change the module kind.

### Debug symbols: `-s`

`-s` requests a portable PDB. If omitted, no debug-symbol file is created.

The PDB contains source document names, SHA-256 source checksums, sequence
points, and local-variable scopes. A document name must be non-empty and cannot
identify sources with different checksums. Reusing a name is allowed when the
checksums are identical.

When `-s` is used without `-debug`, `bc` defaults to debugger-friendly IL. Use
`-debug=false` to emit the PDB while keeping the method bodies closer to the
symbol-free release shape.

### Debugger-friendly IL: `-debug`

`-debug` controls whether `bc` emits debugger-friendly IL. The option accepts a
boolean value such as `true` or `false`.

With `-debug=true`, the emitter adds debug-only NOPs, a shared return epilogue,
local-variable scope information, and `DebuggableAttribute`. This is the default
when `-s` is specified.

With `-debug=false`, `bc` can still emit a portable PDB and sequence points for
real emitted instructions, but optimized-away source locations do not receive
debug-only NOPs. This is useful for builds that need symbols without changing
the generated IL shape as much:

```console
bc program.b -s=out/program.pdb -debug=false
```

Specifying `-debug=true` without `-s` has no practical effect, because no symbol
file or sequence points are emitted.

### Module name: `-m`

`-m` sets the emitted .NET assembly and module name. If omitted, the name is the
file name of the assembly output path without its extension.

This name is .NET metadata. Balu has no source-level module system.

### Quiet mode: `-q`

Quiet mode suppresses all output produced by `bc`, including:

- the compiler banner
- compilation and emission progress
- warnings and errors
- invocation and tool error messages

The process exit code is the only success or failure indication in quiet mode.
Use it only when the caller reliably checks the exit code.

## Compile one source file

This PowerShell example uses the .NET Framework reference assembly from the
repository's development launch profile:

```powershell
New-Item -ItemType Directory -Path "out" -Force

dotnet run --project src/bc -- `
  "src/HelloWorld/hello.b" `
  -r="C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll" `
  -o="out/hello.dll" `
  -s="out/hello.pdb"
```

An equivalent modern .NET invocation supplies each required assembly:

```console
bc program.b \
  -r=<reference-pack>/System.Runtime.dll \
  -r=<reference-pack>/System.Runtime.Extensions.dll \
  -r=<reference-pack>/System.Console.dll \
  -o=out/program.dll \
  -s=out/program.pdb
```

The angle-bracket path is a placeholder and must be replaced with the installed
.NET 10 reference-pack directory.

## Compile multiple source files

Pass every source file as a positional argument:

```console
bc math.b program.b \
  -r=<reference-pack>/System.Runtime.dll \
  -r=<reference-pack>/System.Runtime.Extensions.dll \
  -r=<reference-pack>/System.Console.dll \
  -o=out/program.dll
```

All files form one compilation and share one global namespace. Functions and
global variables can be referenced across file boundaries.

Only one source file may contain global statements. Alternatively, declare a
single valid `main` function. See
[Source files and programs](../language/reference.md#source-files-and-programs)
for the language rules.

Global variable declarations are bound in syntax-tree order, but `bc` currently
parses source paths in unordered parallel processing. It therefore does not
guarantee that cross-file globals are bound in command-line order. Avoid global
initializers that depend on declarations in another file.

## Entry point requirements

A normal compilation needs one of these entry-point forms:

```balu
function main() {
    // Program body
}
```

Or global statements:

```balu
println("Program body")
```

The compiler rejects:

- a `main` with parameters
- a `main` with a return type
- an explicit `main` mixed with global statements
- global statements in more than one file
- a program with neither `main` nor global statements

## Output and file safety

On a successful emit, `bc` writes the assembly and optional PDB. Existing output
files are replaced only after the new output has been emitted successfully.
Compiler or emitter errors leave existing output files intact.

The normalized assembly and PDB paths must differ from each other and from every
source path. Path comparison is case-insensitive on Windows. File-system aliases
such as symbolic links are not resolved when checking collisions.

Reference paths are not included in this collision check. Do not set `-o` or
`-s` to the path of a reference assembly, because a successful emit can replace
that file.

The target directories are not created by `bc`. Create them before invoking the
compiler.

## Console output

A normal successful invocation produces output similar to:

```text
Balu compiler v0.1.0.0
Compiling 'program.b'...
Emitting assembly 'C:\work\out\program.dll' and symbol file 'out/program.pdb'.
Done.
```

With multiple sources, parsing happens in parallel, so the order of
`Compiling ...` messages is not guaranteed.

Source diagnostics are written to standard error in an MSBuild-compatible
format:

```text
C:\work\program.b(3,5): error BL1005: Undefined name 'value'.
    value
```

See [Diagnostics](../language/diagnostics.md) for diagnostic IDs and severities.

Invalid invocations and tool failures are not compiler diagnostics and have no
`BLxxxx` ID or source location. They are written to standard error in this form:

```text
bc: error: Missing required value for option '-o'.
```

`Done.` currently indicates that the compiler completed its normal pipeline;
it can still be printed when diagnostics contain errors. Always use the exit
code to determine success.

## Exit codes

| Exit code | Meaning |
| ---: | --- |
| `0` | Help was displayed, or compilation completed without errors |
| `1` | Compilation completed with error diagnostics |
| `2` | The command-line invocation is invalid, for example because an option value or source file argument is missing |
| `3` | The compiler tool could not complete, for example because an input file could not be read |

Warnings do not change a successful exit code.

## Running the result

Direct `bc` output is an assembly and optional PDB, not a complete application
deployment. A modern .NET host usually also needs framework and runtime metadata
such as a `.runtimeconfig.json` file.

For a normal `dotnet build`, `dotnet run`, or application publishing workflow,
use a project based on `Balu.Sdk`. The SDK invokes `bc` for the intermediate
assembly and then lets the standard .NET SDK produce the remaining application
artifacts.
