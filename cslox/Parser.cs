using System;
using System.Collections.Generic;
using cslox.AST;

namespace cslox
{
  public class Parser
  {
    public class ParseError : Exception { }

    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
      this.tokens = tokens;
    }

    public List<Stmt> Parse()
    {
      List<Stmt> statements = new();

      while (!IsAtEnd())
      {
        statements.Add(Declaration());
      }

      return statements;
    }

    private Stmt Declaration()
    {
      try
      {
        if (Match(TokenType.VAR))
        {
          return VarDeclaration();
        }

        return Statement();
      }
      catch (ParseError)
      {
        Sync();
        return null;
      }
    }

    private Stmt VarDeclaration()
    {
      var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

      Expr initializer = null;
      if (Match(TokenType.EQUAL))
      {
        initializer = Expression();
      }

      Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
      return new Stmt.VarStmt(name, initializer);
    }

    private Stmt Statement()
    {
      if (Match(TokenType.PRINT))
        return PrintStatement();
      if (Match(TokenType.BRACE_LEFT))
        return new Stmt.BlockStmt(Block());

      return ExpressionStatement();
    }

    private Stmt PrintStatement()
    {
      Expr value = Expression();
      Consume(TokenType.SEMICOLON, "Expect ';' after value.");
      return new Stmt.PrintStmt(value);
    }

    private List<Stmt> Block()
    {
      List<Stmt> statements = new();

      while (!Check(TokenType.BRACE_RIGHT) && !IsAtEnd())
      {
        statements.Add(Declaration());
      }

      Consume(TokenType.BRACE_RIGHT, "Expect '}' after block.");
      return statements;
    }

    private Stmt ExpressionStatement()
    {
      Expr expr = Expression();
      Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
      return new Stmt.ExprStmt(expr);
    }
    private Expr Expression()
    {
      return Assignment();
    }

    private Expr Assignment()
    {
      Expr expr = Equality();

      if (Match(TokenType.EQUAL))
      {
        var equals = Previous();
        var value = Assignment();

        if (expr is Expr.Variable variable)
        {
          var name = variable.Name;
          return new Expr.Assign(name, value);
        }

        Err(equals, "Invalid assignment target.");
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

      return Primary();
    }

    private Expr Primary()
    {
      if (Match(TokenType.FALSE)) return new Expr.Literal(false);
      if (Match(TokenType.TRUE)) return new Expr.Literal(true);
      if (Match(TokenType.NIL)) return new Expr.Literal(null);

      if (MatchAny(new TokenType[] { TokenType.NUMBER, TokenType.STRING }))
      {
        return new Expr.Literal(Previous().Literal);
      }

      if (Match(TokenType.IDENTIFIER))
      {
        return new Expr.Variable(Previous());
      }

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
      if (Check(type)) return Advance();

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
      if (IsAtEnd()) return false;
      return Peek().Type == type;
    }

    private Token Advance()
    {
      if (!IsAtEnd()) current++;
      return Previous();
    }

    private bool IsAtEnd()
    {
      return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
      return tokens[current];
    }

    private Token Previous()
    {
      return tokens[current - 1];
    }

    private void Sync()
    {
      Advance();

      while (!IsAtEnd())
      {
        if (Previous().Type == TokenType.SEMICOLON) return;

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