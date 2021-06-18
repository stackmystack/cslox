using System;
using System.IO;
using cslox;

namespace GenAST
{
  class Program
  {
    public static void Main(string[] args)
    {
      if (args.Length != 1)
      {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("gen_ast [output dir]");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Practically, from the root of the project, do this:");
        Console.Error.WriteLine();
        Console.Error.WriteLine("    dotnet run -p gen_ast -- cslox\\AST");
        Environment.Exit(SysExitCode.USAGE);
      }

      var outputDir = args[0];

      DefineAST(outputDir, "Expr", new string[] {
                    "Assign   : Token name, Expr value",
                    "Binary   : Expr left, Token op, Expr right",
                    "Call     : Expr callee, Token paren, List<Expr> arguments",
                    "Grouping : Expr expression",
                    "Literal  : object value",
                    "Logical  : Expr left, Token op, Expr right",
                    "Unary    : Token op, Expr right",
                    "Variable : Token name",
                });

      DefineAST(outputDir, "Stmt", new string[] {
                "Block      : List<Stmt> statements",
                "Expression : Expr expr",
                "Function   : Token name, List<Token> parameters, List<Stmt> body",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print      : Expr expr",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body",
            });
    }

    private static void DefineAST(string outputDir, string baseName, string[] types)
    {
      var path = Path.Combine(outputDir, baseName + ".cs");

      using StreamWriter sw = new(path);
      sw.WriteLine("using System;");
      sw.WriteLine("using System.Collections.Generic;");
      sw.WriteLine();
      sw.WriteLine("namespace cslox.AST");
      sw.WriteLine("{");
      sw.WriteLine("    public abstract class " + baseName);
      sw.WriteLine("    {");

      DefineVisitor(sw, baseName, types);

      foreach (var type in types)
      {
        var className = type.Split(":")[0].Trim();
        var fields = type.Split(":")[1].Trim();
        DefineType(sw, baseName, className, fields);
      }

      sw.WriteLine();
      sw.WriteLine("    public abstract R Accept<R>(IVisitor<R> visitor);");

      sw.WriteLine("    }");
      sw.WriteLine("}");
    }

    private static void DefineVisitor(StreamWriter sw, string baseName, string[] types)
    {
      sw.WriteLine("        public interface IVisitor<R>");
      sw.WriteLine("        {");

      foreach (var type in types)
      {
        var typeName = type.Split(":")[0].Trim();
        sw.WriteLine("            R Visit" + typeName + baseName + "("
                        + typeName + " " + baseName.ToLower() + ");");
      }

      sw.WriteLine("        }");
    }

    private static void DefineType(StreamWriter sw, string baseName, string className, string fieldsList)
    {
      sw.WriteLine("        public class " + className + " : " + baseName);
      sw.WriteLine("        {");
      sw.WriteLine("            public " + className + "(" + fieldsList + ")");
      sw.WriteLine("            {");

      var fields = fieldsList.Split(", ");

      foreach (var field in fields)
      {
        var name = field.Split(" ")[1];
        sw.WriteLine("                " + Prop(name) + " = " + name + ";");
      }
      sw.WriteLine("            }");

      sw.WriteLine();
      sw.WriteLine("            public override R Accept<R>(IVisitor<R> visitor)");
      sw.WriteLine("            {");
      sw.WriteLine("                 return visitor.Visit" + className + baseName + "(this);");
      sw.WriteLine("            }");

      sw.WriteLine();
      foreach (var field in fields)
      {
        var type = field.Split(" ")[0];
        var name = field.Split(" ")[1];
        sw.WriteLine("            public " + type + " " + Prop(name) + " { get; }");
      }
      sw.WriteLine("        }");
    }

    private static string Prop(string name)
    {
      return char.ToUpper(name[0]) + name[1..];
    }
  }
}
