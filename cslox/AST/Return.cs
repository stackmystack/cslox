using System;

namespace cslox.AST
{
  public class Return : Exception
  {
    public readonly object Value;

    public Return(object value)
    {
      Value = value;
    }
  }
}