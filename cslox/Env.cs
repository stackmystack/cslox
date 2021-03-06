using System;
using System.Collections.Generic;

namespace cslox
{
  public class Env
  {
    public Env Enclosing { get; set; }
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
        Values.Remove(name);

      Values.Add(name, value);
    }

    public object Get(Token name)
    {
      var lexeme = name.Lexeme;

      if (Values.ContainsKey(lexeme))
        return Values[lexeme];

      if (Enclosing != null)
        return Enclosing.Get(name);

      throw new RuntimeError(name, "Undefined variable '" + lexeme + "'.");
    }

    public object GetAt(int distance, string name)
    {
      return Ancestor(distance).Values[name];
    }

    public Env Ancestor(int distance)
    {
      Env env = this;

      for (int i = 0; i < distance; i++)
      {
        env = env.Enclosing;
      }

      return env;
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

    public void AssignAt(int distance, Token name, object value)
    {
      Ancestor(distance).Values[name.Lexeme] = value;
    }
  }
}