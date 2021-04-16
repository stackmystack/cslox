using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace cslox
{
    class Program
    {
        static bool hadError = false;
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  cslox             | runs the repl");
            Console.WriteLine("  cslox [FILE]      | runs a file");
        }

        static void RunFile(string path)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            Run(text);

            if (hadError)
                Environment.Exit(SysExitCode.DATAERR);
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
            foreach (var token in scanner.Tokens)
            {
                Console.WriteLine(token);
            }
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
