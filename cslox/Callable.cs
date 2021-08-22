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
    private readonly bool IsInit;

    public Function(Stmt.Function declaration, Env closure, bool isInit = false)
    {
      Declaration = declaration;
      Closure = closure;
      IsInit = isInit;
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
        if (IsInit)
        {
          return Closure.GetAt(0, "this");
        }
        return returnValue.Value;
      }

      if (IsInit)
        return Closure.GetAt(0, "this");

      return null;
    }

    public Function Bind(LoxInstance instance)
    {
      var env = new Env(Closure);
      env.Define("this", instance);
      return new Function(Declaration, env, IsInit);
    }

    public override string ToString()
    {
      return $"<fn {Declaration.Name.Lexeme}>";
    }
  }
}