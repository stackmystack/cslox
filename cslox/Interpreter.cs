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
          if (statement != null)
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

    public object VisitLogicalExpr(Expr.Logical expr)
    {
      var left = Eval(expr.Left);

      if (expr.Op.Type == TokenType.OR)
      {
        if (IsTruthy(left))
          return left;
      }
      else
      {
        if (!IsTruthy(left))
          return left;
      }

      return Eval(expr.Right);
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

    public object VisitBlockStmt(Stmt.Block stmt)
    {
      ExecuteBlock(stmt.Statements, new Env(environment));
      return null;
    }

    private void ExecuteBlock(List<Stmt> statements, Env environment)
    {
      var previous = this.environment;

      try
      {
        this.environment = environment;

        foreach (Stmt statement in statements)
        {
          Execute(statement);
        }
      }
      finally
      {
        this.environment = previous;
      }
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
      Eval(stmt.Expr);
      return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
      if (IsTruthy(Eval(stmt.Condition)))
      {
        Execute(stmt.ThenBranch);
      }
      else if (stmt.ElseBranch != null)
      {
        Execute(stmt.ElseBranch);
      }

      return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
      object value = Eval(stmt.Expr);
      Console.WriteLine(Stringify(value));
      return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
      while (IsTruthy(Eval(stmt.Condition)))
      {
        Execute(stmt.Body);
      }

      return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
      object value = null;

      if (stmt.Initializer != null)
      {
        value = Eval(stmt.Initializer);
      }

      environment.Define(stmt.Name.Lexeme, value);
      return null;
    }

    private static void CheckNumberOperand(Token op, object right)
    {
      if (right is double) return;
      throw new RuntimeError(op, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token op, object left, object right)
    {
      if (right is double && right is double) return;
      throw new RuntimeError(op, "Operands must be a number.");
    }

    private object Eval(Expr expression)
    {
      return expression.Accept(this);
    }

    private static bool IsTruthy(object o)
    {
      if (o == null) return false;
      if (o is bool boolean) return boolean;

      return true;
    }

    private static bool IsEqual(object left, object right)
    {
      if (left == null && right == null) return true;
      if (left == null) return false;

      return left.Equals(right);
    }

  }
}