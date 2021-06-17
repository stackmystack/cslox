# CSLOX

CSLOX is an implementation of `jlox` from [Crafting
Interpreters](https://craftinginterpreters.com).

## `jlox` is written in Java. Why C# not Java?

Why not? I know Java, and I took this opportunity to refresh my knowledge
on C#. The last time I touched c# was in 2010.

## Build

This project uses `.NET Core 5.0`. I guess it's `.NET 5.0` plain and simple
nowadays so it should run on any OS supporting it. I am using it on Windows.

## Run

### Through `dotnet` command

To run the repl:

```bash
dotnet run -p cslox
```

To run a script:

```bash
dotnet run -p cslox -- lox_scripts/example.lox
```

### Standalone

```text
Usage:
  cslox             | runs the repl
  cslox [FILE]      | runs a file
```
