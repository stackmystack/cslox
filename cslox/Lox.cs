using System;
using System.IO;
using System.Text;

namespace cslox
{
  class Lox
  {
    private static bool HadError = false;
    private static bool HadRuntimeError = false;

    private static readonly Interpreter interpreter = new();

    static void PrintUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("  cslox             | runs the repl");
      Console.WriteLine("  cslox [FILE]      | runs a file");
    }

    public static void RuntimeError(RuntimeError e)
    {
      Console.Error.WriteLine($"[line {e.Token.Line}] {e.Message}");
      HadRuntimeError = true;
    }

    static void RunFile(string path)
    {
      var text = File.ReadAllText(path, Encoding.UTF8);
      Run(text);

      if (HadError)
        Environment.Exit(SysExitCode.DATAERR);
      if (HadRuntimeError)
        Environment.Exit(SysExitCode.SOFTWARE);
    }

    static void RunREPL()
    {
      while (true)
      {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line == null) break;
        Run(line);
      }
    }

    static void Run(string program)
    {
      var scanner = new Scanner(program);

      scanner.ScanTokens();

      var parser = new Parser(scanner.Tokens);
      var statements = parser.Parse();

      // Stop here if we had syntactic error
      if (HadError) return;

      var resolver = new Resolver(interpreter);
      resolver.Resolve(statements);

      // Stop if there was a resolution error.
      if (HadError) return;

      interpreter.Interpret(statements);
    }

    static void Main(string[] args)
    {
      if (args.Length > 1)
      {
        PrintUsage();
        Environment.Exit(SysExitCode.USAGE);
      }
      else if (args.Length == 1)
      {
        RunFile(args[0]);
      }
      else
      {
        RunREPL();
      }
    }
  }
}
