# Balu.Sdk

`Balu.Sdk` is the MSBuild SDK for the Balu programming language. It compiles
`.b` source files into managed .NET applications using the Balu compiler.

Balu is an experimental, educational language and currently requires the
.NET 10 SDK.

## Create a project

Create a project file such as `hello.csproj`:

```xml
<Project Sdk="Balu.Sdk/0.6.0">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
</Project>
```

Add a `hello.b` file next to it:

```balu
println("Hello, world!")
```

All `.b` files below the project directory are included automatically. Build
or run the project with the standard .NET CLI:

```console
dotnet build
dotnet run
```

## Learn more

- [Balu repository](https://github.com/ReneVogt/Balu)
- [Getting started](https://github.com/ReneVogt/Balu/blob/main/docs/getting-started.md)
- [Language reference](https://github.com/ReneVogt/Balu/blob/main/docs/language/reference.md)
- [`bc` compiler reference](https://github.com/ReneVogt/Balu/blob/main/docs/tools/compiler.md)

## License

Balu is available under the [MIT License](https://github.com/ReneVogt/Balu/blob/main/LICENSE).
