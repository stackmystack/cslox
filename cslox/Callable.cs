using System.Collections.Generic;
using cslox.AST;
namespace cslox
{
  interface ICallable
  {
    object Call(Interpreter interpreter, List<object> args);
    int Arity();
  }

  class Function : ICallable
  {
    private readonly Stmt.Function Declaration;
    private readonly Env Closure;

    public Function(Stmt.Function declaration, Env closure)
    {
      Declaration = declaration;
      Closure = closure;
    }

    public int Arity()
    {
      return Declaration.Parameters.Count;
    }

    public object Call(Interpreter interpreter, List<object> args)
    {
      var env = new Env(Closure);

      for (int i = 0; i < Declaration.Parameters.Count; ++i)
        env.Define(Declaration.Parameters[i].Lexeme, args[i]);

      try
      {
        interpreter.ExecuteBlock(Declaration.Body, env);
      }
      catch (Return returnValue)
      {
        return returnValue.Value;
      }

      return null;
    }

    public override string ToString()
    {
      return $"<fn {Declaration.Name.Lexeme}>";
    }
  }
}