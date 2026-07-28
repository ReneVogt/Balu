# Balu

Balu is a small programming language and an educational compiler targeting
.NET. It implements the complete pipeline from source text and semantic analysis
to lowered control flow, managed IL, and portable debug symbols.

```balu
function factorial(value: int): int {
    if value <= 1
        return 1

    return value * factorial(value - 1)
}

function main() {
    println(factorial(5))
}
```

## What makes Balu interesting

- A lossless syntax tree with tokens, trivia, and parser recovery
- Static binding, type checking, conversions, and diagnostics
- Lowering of structured control flow into labels and jumps
- Control-flow analysis, return analysis, and unreachable-code detection
- Managed IL and portable PDB emission using Mono.Cecil
- An incremental REPL that compiles each submission into an in-memory assembly
- An MSBuild SDK for compiling `.b` files as .NET projects
- Source-generated syntax and bound-tree visitors and rewriters

## Try it

Balu currently requires the .NET 10 SDK.

Start the interactive REPL from the repository root:

```console
dotnet run --project src/bi
```

Then enter Balu code:

```text
» var value = 21
» value * 2
Result: 42
```

See [Getting started](docs/getting-started.md) for a guided introduction.

## Documentation

- [Documentation overview](docs/index.md)
- [Language overview](docs/language/overview.md)
- [Language reference](docs/language/reference.md)
- [Built-in functions](docs/language/built-in-functions.md)
- [Diagnostics](docs/language/diagnostics.md)
- [`bc` command-line compiler](docs/tools/compiler.md)

## Project status

Balu is an experimental, educational hobby project. The language and its tools
are still evolving, and the documentation describes the current implementation
rather than a stable language specification.

## Background

Balu started as my implementation of the ideas presented in
[terrajobst's Minsk compiler tutorial](https://github.com/terrajobst/minsk).
It has since grown into a complete compiler toolchain with a REPL, IL emission,
debug symbols, an MSBuild SDK, and extensive compiler tests.

## License

Balu is available under the [MIT License](LICENSE).
