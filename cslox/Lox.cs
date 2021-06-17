using System;
using System.IO;
using System.Text;

namespace cslox
{
  class Lox
  {
    private static bool hadError = false;
    private static bool hadRuntimeError = false;

    private static readonly Interpreter interpreter = new();

    static void PrintUsage()
    {
      Console.WriteLine("Usage:");
      Console.WriteLine("  cslox             | runs the repl");
      Console.WriteLine("  cslox [FILE]      | runs a file");
    }

    public static void RuntimeError(RuntimeError e)
    {
      Console.Error.WriteLine(e.Message + "\n [line " + e.Token + "]");
      hadRuntimeError = true;
    }

    static void RunFile(string path)
    {
      var text = File.ReadAllText(path, Encoding.UTF8);
      Run(text);

      if (hadError)
        Environment.Exit(SysExitCode.DATAERR);
      if (hadRuntimeError)
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
      var expr = parser.Parse();

      if (hadError) return;

      interpreter.Interpret(expr);
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
