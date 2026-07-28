# Balu documentation

Balu is a small programming language and compiler inspired by the
[Minsk compiler tutorial](https://github.com/terrajobst/minsk). The project is
primarily educational: it explores the complete path from source text to a
managed .NET assembly, including parsing, semantic analysis, lowering, debug
symbols, and an interactive REPL.

This documentation describes the language and tools as they are implemented
today. Balu is still experimental, so the documentation is a reference to the
current behavior rather than a stable, normative language specification.

## Start here

- [Getting started](getting-started.md) builds and starts the Balu REPL from
  the source repository and introduces the basic language constructs.
- [Language overview](language/overview.md) gives a guided tour of Balu
  programs, types, functions, and control flow.
- [Language reference](language/reference.md) describes the syntax and
  semantics in detail.
- [Built-in functions](language/built-in-functions.md) documents console I/O
  and random-number generation.
- [Diagnostics](language/diagnostics.md) explains Balu's errors, warnings, and
  diagnostic IDs.
- [Compiler command line](tools/compiler.md) is the reference for the `bc`
  command-line compiler.

## Project components

| Component | Purpose |
| --- | --- |
| `Balu` | Compiler library containing the syntax, binding, lowering, diagnostics, and emission pipelines |
| `bc` | Command-line compiler that emits a .NET assembly and optionally a portable PDB |
| `bi` | Interactive REPL for incremental Balu submissions |
| `Balu.Interpretation` | Execution layer used by the REPL and tests |
| `Balu.Sdk` | MSBuild SDK for compiling `.b` files as part of a .NET project |
| `Balu.SourceGenerator` | Build-time generator for syntax and bound-tree infrastructure |
| `Balu.VSIX` | Visual Studio project and item templates |
| `Balu.Tests` | Lexer, parser, binder, execution, emitter, and interpreter tests |

The interpreter is not an AST evaluator. Each successful submission is
compiled into an in-memory .NET assembly, loaded, executed, and then used as
the basis for the next submission.

## Language at a glance

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

Balu currently provides:

- `int`, `bool`, `string`, and `any` values
- mutable `var` and read-only `let` declarations
- global functions with typed parameters and optional return types
- `if`, `while`, `do while`, and inclusive `for ... to ...` control flow
- global statements or a `main` function as the program entry point
- multiple source files in one shared global namespace
- compilation to managed .NET IL

Features such as arrays, user-defined types, modules, imports, exceptions,
floating-point numbers, and generics are not implemented.

## Documentation scope

The language documentation separates three related concepts:

- A **program** is a normal compilation emitted by `bc` or `Balu.Sdk`.
- A **script** is an incremental compilation used by the REPL.
- The **compiler API** is the .NET API exposed by the `Balu` assembly.

Program and script syntax are mostly identical, but their entry points,
top-level expression rules, and state lifetime differ. These differences are
called out where relevant.
