using System;
using System.Collections.Generic;
using cslox.AST;

namespace cslox
{
  public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
  {
    private Env environment = new();

    public void Interpret(List<Stmt> statements)
    {
      try
      {
        foreach (Stmt statement in statements)
        {
          Execute(statement);
        }
      }
      catch (RuntimeError e)
      {
        Lox.RuntimeError(e);
      }
    }

    private void Execute(Stmt stmt)
    {
      stmt.Accept(this);
    }

    private string Stringify(object value)
    {
      if (value == null) return "nil";

      if (value is double)
      {
        var text = value.ToString();

        if (text.EndsWith(".0"))
        {
          text = text[0..(text.Length - 2)];
        }

        return text;
      }

      return value.ToString();
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
      object value = Eval(expr.Value);
      environment.Assign(expr.Name, value);
      return value;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
      var left = Eval(expr.Left);
      var right = Eval(expr.Right);

      switch (expr.Op.Type)
      {
        case TokenType.GT:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left > (double)right;
        case TokenType.GTE:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left >= (double)right;
        case TokenType.LT:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left < (double)right;
        case TokenType.LTE:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left <= (double)right;
        case TokenType.SLASH:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left / (double)right;
        case TokenType.STAR:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left * (double)right;
        case TokenType.MINUS:
          CheckNumberOperands(expr.Op, left, right);
          return (double)left - (double)right;
        case TokenType.PLUS:
          if (left is double l && right is double r)
            return l + r;
          if (left is string ls && right is string rs)
            return ls + rs;
          throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
        case TokenType.BANG_EQUAL: return !IsEqual(left, right);
        case TokenType.EQUAL_EQUAL: return !IsEqual(left, right);
        default:
          return null;
      }
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
      return Eval(expr.Expression);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
      return expr.Value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
      var right = Eval(expr.Right);

      switch (expr.Op.Type)
      {
        case TokenType.BANG: return !IsTruthy(right);
        case TokenType.MINUS:
          CheckNumberOperand(expr.Op, right);
          return -(double)right;
        default: return null;
      }
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
      return environment.Get(expr.Name);
    }

    public object VisitExprStmtStmt(Stmt.ExprStmt stmt)
    {
      Eval(stmt.Expression);
      return null;
    }

    public object VisitPrintStmtStmt(Stmt.PrintStmt stmt)
    {
      object value = Eval(stmt.Expression);
      Console.WriteLine(Stringify(value));
      return null;
    }

    public object VisitVarStmtStmt(Stmt.VarStmt stmt)
    {
      object value = null;

      if (stmt.Initializer != null)
      {
        value = Eval(stmt.Initializer);
      }

      environment.Define(stmt.Name.Lexeme, value);
      return null;
    }

    private void CheckNumberOperand(Token op, object right)
    {
      if (right is double) return;
      throw new RuntimeError(op, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token op, object left, object right)
    {
      if (right is double && right is double) return;
      throw new RuntimeError(op, "Operands must be a number.");
    }

    private object Eval(Expr expression)
    {
      return expression.Accept(this);
    }

    private bool IsTruthy(object o)
    {
      if (o == null) return false;
      if (o is bool boolean) return boolean;

      return true;
    }

    private bool IsEqual(object left, object right)
    {
      if (left == null && right == null) return true;
      if (left == null) return false;

      return left.Equals(right);
    }

  }
}