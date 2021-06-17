using System;
using System.Collections.Generic;

namespace cslox
{
  public class Env
  {
    private Dictionary<string, object> Values;

    public Env()
    {
      Values = new();
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

      throw new RuntimeError(name, "Undefined variable '" + lexeme + "',");
    }
  }
}