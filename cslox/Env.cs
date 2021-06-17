using System;
using System.Collections.Generic;

namespace cslox
{
  public class Env
  {
    private readonly Env Enclosing;
    private readonly Dictionary<string, object> Values;

    public Env() : this(null)
    {
    }

    public Env(Env enclosing)
    {
      Values = new();
      Enclosing = enclosing;
    }

    public void Define(string name, object value)
    {
      if (Values.ContainsKey(name))
      {
        Values.Remove(name);
      }

      Values.Add(name, value);
    }

    public object Get(Token name)
    {
      var lexeme = name.Lexeme;

      if (Values.ContainsKey(lexeme))
      {
        return Values[lexeme];
      }

      if (Enclosing != null)
      {
        return Enclosing.Get(name);
      }

      throw new RuntimeError(name, "Undefined variable '" + lexeme + "'.");
    }

    public void Assign(Token name, object value)
    {
      var lexeme = name.Lexeme;

      if (Values.ContainsKey(lexeme))
      {
        Values.Remove(lexeme);
        Values.Add(lexeme, value);
        return;
      }

      if (Enclosing != null)
      {
        Enclosing.Assign(name, value);
        return;
      }

      throw new RuntimeError(name, "Undefined variable '" + lexeme + "',");
    }
  }
}