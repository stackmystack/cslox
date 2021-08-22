using System;
using System.Collections.Generic;
using cslox.AST;

namespace cslox
{

  class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
  {
    private enum FunctionType
    {
      NONE,
      FUNCTION
    }

    private readonly Interpreter interpreter;
    private Stack<Dictionary<string, bool>> scopes;
    private FunctionType CurrentFunctionType = FunctionType.NONE;

    public Resolver(Interpreter interpreter)
    {
      this.interpreter = interpreter;
      this.scopes = new();
    }

    private void BeginScope()
    {
      scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
      scopes.Pop();
    }

    public void Resolve(List<Stmt> statements)
    {
      foreach (var statement in statements)
      {
        Resolve(statement);
      }
    }

    private void Resolve(Stmt statement)
    {
      statement.Accept(this);
    }

    private void Resolve(Expr expr)
    {
      expr.Accept(this);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
      var idx_max = scopes.Count - 1;
      var idx = idx_max;
      foreach (var scope in scopes)
      {
        if (scope.ContainsKey(name.Lexeme))
        {
          interpreter.Resolve(expr, idx_max - idx);
          return;
        }
        idx -= 1;
      }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
      var enclosingFunctionType = CurrentFunctionType;
      CurrentFunctionType = type;

      BeginScope();

      foreach (Token param in function.Parameters)
      {
        Declare(param);
        Define(param);
      }

      Resolve(function.Body);
      EndScope();

      CurrentFunctionType = enclosingFunctionType;
    }

    private void Declare(Token name)
    {
      if (scopes.Count == 0)
        return;

      var scope = scopes.Peek();

      if (scope.ContainsKey(name.Lexeme))
      {
        Error.Log(name, "Already a variable with this name in the scope");
      }

      scope.Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
      if (scopes.Count == 0)
        return;

      var scope = scopes.Peek();
      scope[name.Lexeme] = true;
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
      Resolve(expr.Value);
      ResolveLocal(expr, expr.Name);
      return null;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
      Resolve(expr.Left);
      Resolve(expr.Right);
      return null;
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
      BeginScope();
      Resolve(stmt.Statements);
      EndScope();
      return null;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
      Resolve(expr.Callee);

      foreach (var arg in expr.Arguments)
      {
        Resolve(arg);
      }

      return null;
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
      Resolve(stmt.Expr);
      return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
      Declare(stmt.Name);
      Define(stmt.Name);

      ResolveFunction(stmt, FunctionType.FUNCTION);
      return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
      Resolve(expr.Expression);
      return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
      Resolve(stmt.Condition);
      Resolve(stmt.ThenBranch);

      if (stmt.ElseBranch != null)
      {
        Resolve(stmt.ElseBranch);
      }

      return null;
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
      return null;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
      Resolve(expr.Right);
      Resolve(expr.Left);
      return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
      Resolve(stmt.Expr);
      return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
      if (CurrentFunctionType == FunctionType.NONE)
      {
        Error.Log(stmt.Keyword, "Can't return from top-level code.");
      }

      if (stmt.Value != null)
      {
        Resolve(stmt.Value);
      }

      return null;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
      Resolve(expr.Right);
      return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
      if (scopes.Count > 0 &&
         scopes.Peek().GetValueOrDefault(expr.Name.Lexeme, false) == false)
      {
        Error.Log(expr.Name, "Can't read local variable in its own initializer.");
      }

      ResolveLocal(expr, expr.Name);
      return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
      Declare(stmt.Name);

      if (stmt.Initializer != null)
      {
        Resolve(stmt.Initializer);
      }

      Define(stmt.Name);
      return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
      Resolve(stmt.Condition);
      Resolve(stmt.Body);
      return null;
    }
  }
}