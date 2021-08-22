using System;
using System.Collections.Generic;
using cslox.AST;

namespace cslox
{
  public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
  {
    public readonly Env Globals;
    private Env Environment;
    private Dictionary<Expr, int> locals;

    private class Clock : ICallable
    {
      public int Arity() { return 0; }

      public object Call(Interpreter interpreter, List<object> args)
      {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000.0;
      }

      public override string ToString()
      {
        return "<native fn>";
      }
    }

    public Interpreter()
    {
      Globals = new();
      Environment = Globals;
      locals = new();

      Globals.Define("clock", new Clock());
    }

    public void Interpret(List<Stmt> statements)
    {
      try
      {
        foreach (Stmt statement in statements)
          if (statement != null)
            Execute(statement);
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

    private static string Stringify(object value)
    {
      if (value == null) return "nil";

      if (value is double)
      {
        var text = value.ToString();

        if (text.EndsWith(".0"))
          text = text[0..(text.Length - 2)];

        return text;
      }

      return value.ToString();
    }

    public void Resolve(Expr expr, int depth)
    {
      locals.Add(expr, depth);
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
      object value = Eval(expr.Value);

      if (!locals.ContainsKey(expr))
      {
        Globals.Assign(expr.Name, value);
      }
      else
      {
        var distance = locals[expr];
        Environment.AssignAt(distance, expr.Name, value);
      }

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

    public object VisitCallExpr(Expr.Call expr)
    {
      var callee = Eval(expr.Callee);

      if (callee is not ICallable)
        throw new RuntimeError(expr.Paren, "Can only call functions and classes.");

      var fn = callee as ICallable;
      List<object> args = new();
      foreach (var arg in expr.Arguments)
        args.Add(Eval(arg));

      if (args.Count != fn.Arity())
        throw new RuntimeError(expr.Paren, $"Expected {fn.Arity()} arguments but got {args.Count}.");

      return fn.Call(this, args);
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
      return LookupVariable(expr.Name, expr);
    }

    private object LookupVariable(Token name, Expr expr)
    {
      if (!locals.ContainsKey(expr))
      {
        return Globals.Get(name);
      }

      var distance = locals[expr];
      return Environment.GetAt(distance, name.Lexeme);
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
      ExecuteBlock(stmt.Statements, new Env(Environment));
      return null;
    }

    public void ExecuteBlock(List<Stmt> statements, Env environment)
    {
      var previous = this.Environment;

      try
      {
        this.Environment = environment;

        foreach (Stmt statement in statements)
          Execute(statement);
      }
      finally
      {
        this.Environment = previous;
      }
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
      Eval(stmt.Expr);
      return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
      var fn = new Function(stmt, Environment);
      Environment.Define(stmt.Name.Lexeme, fn);
      return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
      if (IsTruthy(Eval(stmt.Condition)))
        Execute(stmt.ThenBranch);
      else if (stmt.ElseBranch != null)
        Execute(stmt.ElseBranch);

      return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
      object value = Eval(stmt.Expr);
      Console.WriteLine(Stringify(value));
      return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
      object value = null;
      if (stmt.Value != null)
        value = Eval(stmt.Value);

      throw new Return(value);
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
      while (IsTruthy(Eval(stmt.Condition)))
        Execute(stmt.Body);

      return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
      object value = null;

      if (stmt.Initializer != null)
        value = Eval(stmt.Initializer);

      Environment.Define(stmt.Name.Lexeme, value);
      return null;
    }

    private static void CheckNumberOperand(Token op, object right)
    {
      if (right is double)
        return;

      throw new RuntimeError(op, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token op, object left, object right)
    {
      if (right is double && right is double)
        return;

      throw new RuntimeError(op, "Operands must be a number.");
    }

    private object Eval(Expr expression)
    {
      return expression.Accept(this);
    }

    private static bool IsTruthy(object o)
    {
      if (o == null)
        return false;

      if (o is bool boolean)
        return boolean;

      return true;
    }

    private static bool IsEqual(object left, object right)
    {
      if (left == null && right == null)
        return true;
      if (left == null)
        return false;

      return left.Equals(right);
    }
  }
}