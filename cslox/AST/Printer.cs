using System;
using System.Collections.Generic;
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
      return Paren(expr.Name.Lexeme, new() { expr.Value });
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
      return Paren(expr.Op.Lexeme, new() { expr.Left, expr.Right });
    }

    public string VisitCallExpr(Expr.Call expr)
    {
      var list = new List<Expr>(capacity: 1 + expr.Arguments.Count)
      {
        expr.Callee
      };

      foreach (var arg in expr.Arguments)
      {
        list.Add(arg);
      }

      return Paren("fn", list);
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
      return Paren("group", new() { expr.Expression });
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
      return Paren(expr.Op.ToString(), new() { expr.Left, expr.Right });
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
      if (expr.Value == null) return "nil";
      return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
      return Paren(expr.Op.Lexeme, new() { expr.Right });
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
      return Paren(expr.Name.Lexeme, new());
    }

    private string Paren(string name, List<Expr> exprs)
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