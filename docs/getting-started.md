# Getting started

This guide runs Balu directly from the source repository and uses the
interactive REPL as the shortest path to a working program. It does not require
installing `Balu.Sdk` or manually selecting .NET reference assemblies.

## Prerequisites

- Git
- The .NET 10 SDK
- A terminal with interactive console support

List the installed .NET SDKs:

```console
dotnet --list-sdks
```

The installed SDK list must include a .NET 10 SDK.

## Start the REPL

Run the following command from the repository root:

```console
dotnet run --project src/bi
```

The first run restores and builds the required projects. The `»` prompt accepts
Balu code:

```text
» 1 + 2
Result: 3
```

The REPL compiles each submission and keeps the state of successful
submissions.

## Values and variables

Balu has integer, Boolean, string, and `any` values. An `any` value keeps a
value without exposing its more specific type to static operations; convert it
back to a specific type before using type-specific operators:

```text
» 42
Result: 42
» true
Result: True
» "Hello, Balu!"
Result: "Hello, Balu!"
```

Declare a mutable variable with `var`:

```text
» var count = 1
» count += 4
Result: 5
» count
Result: 5
```

Declare a read-only value with `let`:

```text
» let answer = 42
» answer
Result: 42
```

Types are normally inferred from the initializer. They can also be written
explicitly:

```balu
var name: string = "Balu"
let enabled: bool = true
```

## Define a function

Functions use typed parameters. Add a return type after the parameter list when
the function returns a value:

```balu
function square(value: int): int {
    return value * value
}
```

After submitting the declaration, call the function:

```text
» square(6)
Result: 36
```

The REPL normally continues onto another line while a construct is incomplete.
Use `Ctrl+Enter` to insert a new line even when the current submission could
already be executed.

## Use control flow

Conditions do not require parentheses. Blocks use braces, and statements do
not end with semicolons:

```balu
var total = 0

for i = 1 to 5 {
    total += i
}

if total == 15
    println("The total is 15.")
else
    println("Unexpected total.")
```

The upper bound of `for ... to ...` is inclusive, so this loop visits the
values `1`, `2`, `3`, `4`, and `5`.

## Console input and output

Use `print` or `println` for output and `input` for reading a line:

```balu
print("What is your name? ")
var name = input()
println("Hello, " + name)
```

See [Built-in functions](language/built-in-functions.md) for all built-ins and
their signatures.

## Useful REPL commands

REPL commands start with `#` and are case-sensitive:

| Command | Purpose |
| --- | --- |
| `#help` | List all available commands |
| `#ls` | List visible variables and functions |
| `#showSyntax` | Toggle syntax-tree output |
| `#showProgram` | Toggle bound-program output |
| `#showVars` | Toggle global-variable output after evaluation |
| `#reset` | Reset the interpreter and delete persisted submissions |
| `#exit` | Exit the REPL |

On Windows, successful submissions are stored below
`%LOCALAPPDATA%\Balu\Submissions`. On other platforms, they are stored below the
platform's local application-data directory. They are loaded again when the
REPL starts. Loading means executing the submissions again, so previous console
output, calls to `input()`, and other side effects can occur again. Use
`#reset` when you want to discard that state.

`#clearHistory` only clears the in-memory editor history. It does not reset the
interpreter and does not delete persisted submissions.

## Next steps

- Read the [language overview](language/overview.md) for a broader tour.
- Use the [language reference](language/reference.md) for exact syntax and type
  rules.
- Use the [compiler command-line reference](tools/compiler.md) to compile `.b`
  files outside the REPL.
