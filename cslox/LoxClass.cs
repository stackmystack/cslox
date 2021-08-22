using System;
using System.Collections.Generic;

namespace cslox
{
  class LoxClass : ICallable
  {
    public readonly string Name;
    public readonly Dictionary<string, Function> methods;

    public LoxClass(string name, Dictionary<string, Function> methods)
    {
      this.Name = name;
      this.methods = methods;
    }

    public Function FindMethod(string name)
    {
      return this.methods.GetValueOrDefault(name, null);
    }

    public int Arity()
    {
      var init = FindMethod("init");
      if (init == null)
        return 0;
      return init.Arity();
    }

    public object Call(Interpreter interpreter, List<object> args)
    {
      var instance = new LoxInstance(this);

      var init = FindMethod("init");
      if (init != null)
      {
        init.Bind(instance).Call(interpreter, args);
      }

      return instance;
    }

    public override string ToString()
    {
      return Name;
    }
  }

  class LoxInstance
  {
    private readonly LoxClass klass;
    private readonly Dictionary<string, object> fields;

    public LoxInstance(LoxClass klass)
    {
      this.klass = klass;
      fields = new();
    }

    public object Get(Token name)
    {
      if (fields.ContainsKey(name.Lexeme))
      {
        return fields[name.Lexeme];
      }

      var method = klass.FindMethod(name.Lexeme);
      if (method != null)
      {
        return method.Bind(this);
      }

      throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
    }

    public void Set(Token name, object value)
    {
      fields[name.Lexeme] = value;
    }

    public override string ToString()
    {
      return klass.Name + " instance";
    }
  }
}