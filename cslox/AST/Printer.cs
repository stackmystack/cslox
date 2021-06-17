using System;
using System.Text;

namespace cslox.AST
{
  class ASTPrinter : Expr.IVisitor<string>
  {
    public string Print(Expr expr)
    {
      if (expr == null)
        return "nil";
      return expr.Accept(this);
    }

    public string VisitAssignExpr(Expr.Assign expr)
    {
      return Paren(expr.Name.Lexeme, new Expr[] { expr.Value });
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
      return Paren(expr.Op.Lexeme, new Expr[] { expr.Left, expr.Right });
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
      return Paren("group", new Expr[] { expr.Expression });
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
      if (expr.Value == null) return "nil";
      return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
      return Paren(expr.Op.Lexeme, new Expr[] { expr.Right });
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
      return Paren(expr.Name.Lexeme, Array.Empty<Expr>());
    }

    private string Paren(string name, Expr[] exprs)
    {
      var sb = new StringBuilder();

      sb.Append('(').Append(name);
      foreach (var expr in exprs)
      {
        sb.Append(' ');
        sb.Append(expr.Accept(this));
      }
      sb.Append(')');

      return sb.ToString();
    }
  }
}