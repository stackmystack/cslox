using System;
using System.Collections.Generic;
using cslox.AST;

namespace cslox
{
  public class Parser
  {
    public class ParseError : Exception { }

    private readonly List<Token> Tokens;
    private int Current = 0;

    public Parser(List<Token> tokens)
    {
      this.Tokens = tokens;
    }

    public List<Stmt> Parse()
    {
      List<Stmt> statements = new();

      while (!IsAtEnd())
        statements.Add(Declaration());

      return statements;
    }

    private Stmt Declaration()
    {
      try
      {
        if (Match(TokenType.CLASS))
          return ClassDeclaration();
        if (Match(TokenType.FUN))
          return Function("function");
        if (Match(TokenType.VAR))
          return VarDeclaration();

        return Statement();
      }
      catch (ParseError)
      {
        Sync();
        return null;
      }
    }

    private Stmt Function(string kind)
    {
      var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

      Consume(TokenType.PAREN_LEFT, $"Expect '(' after {kind} name.");

      List<Token> parameters = new();

      if (!Check(TokenType.PAREN_RIGHT))
      {
        do
        {
          if (parameters.Count >= 255)
          {
            Err(Peek(), "Can't have more than 255 parameters.");
          }

          parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
        }
        while (Match(TokenType.COMMA));
      }

      Consume(TokenType.PAREN_RIGHT, "Expect ')' after parameters.");
      Consume(TokenType.BRACE_LEFT, $"Expect '{{' before {kind} body.");

      var body = Block();

      return new Stmt.Function(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
      var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

      Expr initializer = null;
      if (Match(TokenType.EQUAL))
        initializer = Expression();

      Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
      return new Stmt.Var(name, initializer);
    }

    private Stmt ClassDeclaration()
    {
      var name = Consume(TokenType.IDENTIFIER, "Expect class name.");
      Consume(TokenType.BRACE_LEFT, "Expect '{' before class body.");

      var methods = new List<Stmt.Function>();

      while (!Check(TokenType.BRACE_RIGHT) && !IsAtEnd())
      {
        methods.Add(Function("method") as Stmt.Function);
      }

      Consume(TokenType.BRACE_RIGHT, "Expect '}' after class body");

      return new Stmt.Class(name, methods);
    }

    private Stmt Statement()
    {
      if (Match(TokenType.FOR))
        return ForStatement();
      if (Match(TokenType.IF))
        return IfStatement();
      if (Match(TokenType.PRINT))
        return PrintStatement();
      if (Match(TokenType.RETURN))
        return ReturnStatement();
      if (Match(TokenType.WHILE))
        return WhileStatement();
      if (Match(TokenType.BRACE_LEFT))
        return new Stmt.Block(Block());

      return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
      Consume(TokenType.PAREN_LEFT, "Expect '(' after for.");

      Stmt initializer;
      if (Match(TokenType.SEMICOLON))
        initializer = null;
      else if (Match(TokenType.VAR))
        initializer = VarDeclaration();
      else
        initializer = ExpressionStatement();

      Expr condition = null;
      if (!Check(TokenType.SEMICOLON))
        condition = Expression();
      Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

      Expr increment = null;
      if (!Check(TokenType.PAREN_RIGHT))
        increment = Expression();
      Consume(TokenType.PAREN_RIGHT, "Expect ')' after for clauses.");

      Stmt body = Statement();
      if (increment != null)
        body = new Stmt.Block(new List<Stmt>() { body, new Stmt.Expression(increment) });

      if (condition == null)
        condition = new Expr.Literal(true);

      body = new Stmt.While(condition, body);

      if (initializer != null)
        body = new Stmt.Block(new List<Stmt>() { initializer, body });

      return body;
    }

    private Stmt IfStatement()
    {
      Consume(TokenType.PAREN_LEFT, "Expect '(' after 'if'.");
      var condition = Expression();
      Consume(TokenType.PAREN_RIGHT, "Expect ')' after 'if'.");

      Stmt thenBranch = Statement();
      Stmt elseBranch = null;

      if (Match(TokenType.ELSE))
        elseBranch = Statement();

      return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
      Expr value = Expression();
      Consume(TokenType.SEMICOLON, "Expect ';' after value.");
      return new Stmt.Print(value);
    }

    private Stmt ReturnStatement()
    {
      var keyword = Previous();
      Expr value = null;

      if (!Check(TokenType.SEMICOLON))
        value = Expression();

      Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
      return new Stmt.Return(keyword, value);
    }

    private Stmt WhileStatement()
    {
      Consume(TokenType.PAREN_LEFT, "Expect '(' after 'while'.");
      var condition = Expression();
      Consume(TokenType.PAREN_RIGHT, "Expect '(' after 'while'.");
      var body = Statement();

      return new Stmt.While(condition, body);
    }

    private List<Stmt> Block()
    {
      List<Stmt> statements = new();

      while (!Check(TokenType.BRACE_RIGHT) && !IsAtEnd())
        statements.Add(Declaration());

      Consume(TokenType.BRACE_RIGHT, "Expect '}' after block.");
      return statements;
    }

    private Stmt ExpressionStatement()
    {
      Expr expr = Expression();
      Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
      return new Stmt.Expression(expr);
    }
    private Expr Expression()
    {
      return Assignment();
    }

    private Expr Assignment()
    {
      var expr = Or();

      if (Match(TokenType.EQUAL))
      {
        var equals = Previous();
        var value = Assignment();

        if (expr is Expr.Variable variable)
        {
          var name = variable.Name;
          return new Expr.Assign(name, value);
        }
        else if (expr is Expr.Get get)
        {
          return new Expr.Set(get.Obj, get.Name, value);
        }

        Err(equals, "Invalid assignment target.");
      }

      return expr;
    }

    private Expr Or()
    {
      var expr = And();

      while (Match(TokenType.OR))
      {
        var op = Previous();
        var right = And();

        expr = new Expr.Logical(expr, op, right);
      }

      return expr;
    }

    private Expr And()
    {
      var expr = Equality();

      while (Match(TokenType.AND))
      {
        var op = Previous();
        var right = Equality();

        expr = new Expr.Logical(expr, op, right);
      }

      return expr;
    }

    private Expr Equality()
    {
      var expr = Comparison();

      while (MatchAny(new TokenType[] { TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL }))
      {
        var op = Previous();
        var right = Comparison();

        expr = new Expr.Binary(expr, op, right);
      }

      return expr;
    }

    private Expr Comparison()
    {
      var expr = Term();

      while (MatchAny(new TokenType[] { TokenType.GT, TokenType.GTE, TokenType.LT, TokenType.LTE }))
      {
        var op = Previous();
        var right = Term();

        expr = new Expr.Binary(expr, op, right);
      }

      return expr;
    }

    private Expr Term()
    {
      var expr = Factor();

      while (MatchAny(new TokenType[] { TokenType.MINUS, TokenType.PLUS }))
      {
        var op = Previous();
        var right = Factor();

        expr = new Expr.Binary(expr, op, right);
      }

      return expr;
    }

    private Expr Factor()
    {
      var expr = Unary();

      while (MatchAny(new TokenType[] { TokenType.SLASH, TokenType.STAR }))
      {
        var op = Previous();
        var right = Unary();

        expr = new Expr.Binary(expr, op, right);
      }

      return expr;
    }

    private Expr Unary()
    {
      if (MatchAny(new TokenType[] { TokenType.BANG, TokenType.MINUS }))
      {
        var op = Previous();
        var right = Unary();

        return new Expr.Unary(op, right);
      }

      return Call();
    }

    private Expr Call()
    {
      var expr = Primary();

      while (true)
      {
        if (Match(TokenType.PAREN_LEFT))
        {
          expr = FinishCall(expr);
        }
        else if (Match(TokenType.DOT))
        {
          var name = Consume(TokenType.IDENTIFIER, "Expect property name after '.' .");
          expr = new Expr.Get(expr, name);
        }
        else
        {
          break;
        }
      }

      return expr;
    }

    private Expr FinishCall(Expr callee)
    {
      List<Expr> args = new();

      if (!Check(TokenType.PAREN_RIGHT))
      {
        do
        {
          if (args.Count >= 255)
            Err(Peek(), "Can't have more than 255 arguments.");
          args.Add(Expression());
        }
        while (Match(TokenType.COMMA));
      }

      var paren = Consume(TokenType.PAREN_RIGHT, "Expect ')' after arguments.");

      return new Expr.Call(callee, paren, args);
    }

    private Expr Primary()
    {
      if (Match(TokenType.FALSE)) return new Expr.Literal(false);
      if (Match(TokenType.TRUE)) return new Expr.Literal(true);
      if (Match(TokenType.NIL)) return new Expr.Literal(null);

      if (MatchAny(new TokenType[] { TokenType.NUMBER, TokenType.STRING }))
        return new Expr.Literal(Previous().Literal);

      if (Match(TokenType.THIS))
        return new Expr.This(Previous());

      if (Match(TokenType.IDENTIFIER))
        return new Expr.Variable(Previous());

      if (Match(TokenType.PAREN_LEFT))
      {
        var expr = Expression();
        Consume(TokenType.PAREN_RIGHT, "Expect ')' after expression.");

        return new Expr.Grouping(expr);
      }

      throw Err(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
      if (Check(type))
        return Advance();

      throw Err(Peek(), message);
    }

    private static ParseError Err(Token token, string message)
    {
      Error.Log(token, message);
      return new ParseError();
    }

    private bool MatchAny(TokenType[] types)
    {
      foreach (var type in types)
      {
        if (Check(type))
        {
          Advance();
          return true;
        }
      }

      return false;
    }

    private bool Match(TokenType type)
    {
      if (Check(type))
      {
        Advance();
        return true;
      }

      return false;
    }

    private bool Check(TokenType type)
    {
      if (IsAtEnd())
        return false;
      return Peek().Type == type;
    }

    private Token Advance()
    {

      if (!IsAtEnd())
        Current++;
      return Previous();
    }

    private bool IsAtEnd()
    {
      return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
      return Tokens[Current];
    }

    private Token Previous()
    {
      return Tokens[Current - 1];
    }

    private void Sync()
    {
      Advance();

      while (!IsAtEnd())
      {
        if (Previous().Type == TokenType.SEMICOLON)
          return;

        switch (Peek().Type)
        {
          case TokenType.CLASS:
          case TokenType.FUN:
          case TokenType.VAR:
          case TokenType.FOR:
          case TokenType.IF:
          case TokenType.WHILE:
          case TokenType.PRINT:
          case TokenType.RETURN: return;
        }

        Advance();
      }
    }

  }
}