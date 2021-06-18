using System;
using System.Collections.Generic;

namespace cslox
{
  public class Scanner
  {
    private int Start;
    private int Current;
    private int Line;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
      { "and", TokenType.AND },
      { "class", TokenType.CLASS },
      { "else", TokenType.ELSE },
      { "false", TokenType.FALSE },
      { "for", TokenType.FOR },
      { "fun", TokenType.FUN },
      { "if", TokenType.IF },
      { "nil", TokenType.NIL },
      { "or", TokenType.OR },
      { "print", TokenType.PRINT },
      { "return", TokenType.RETURN },
      { "super", TokenType.SUPER },
      { "this", TokenType.THIS },
      { "true", TokenType.TRUE },
      { "var", TokenType.VAR },
      { "while", TokenType.WHILE },
    };

    public string Source { get; }
    public List<Token> Tokens { get; }

    private Scanner() { }

    public Scanner(string source)
    {
      Tokens = new List<Token>();
      Source = source;
      Start = 0;
      Current = 0;
      Line = 1;
    }

    public void ScanTokens()
    {
      while (!IsAtEnd())
      {
        Start = Current;
        ScanSingleToken();
      }

      Tokens.Add(new Token(TokenType.EOF, "", null, Line));
    }

    private bool IsAtEnd()
    {
      return Current >= Source.Length;
    }

    private void ScanSingleToken()
    {
      var c = Advance();

      switch (c)
      {
        case '(': AddToken(TokenType.PAREN_LEFT); break;
        case ')': AddToken(TokenType.PAREN_RIGHT); break;
        case '{': AddToken(TokenType.BRACE_LEFT); break;
        case '}': AddToken(TokenType.BRACE_RIGHT); break;
        case ',': AddToken(TokenType.COMMA); break;
        case '.': AddToken(TokenType.DOT); break;
        case '-': AddToken(TokenType.MINUS); break;
        case '+': AddToken(TokenType.PLUS); break;
        case ';': AddToken(TokenType.SEMICOLON); break;
        case '*': AddToken(TokenType.STAR); break;
        case '!':
          AddToken(CanMatch('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
          break;
        case '=':
          AddToken(CanMatch('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
          break;
        case '<':
          AddToken(CanMatch('=') ? TokenType.LTE : TokenType.LT);
          break;
        case '>':
          AddToken(CanMatch('=') ? TokenType.GTE : TokenType.GT);
          break;
        case '/':
          if (CanMatch('/'))
          {
            while (Peek() != '\n' && !IsAtEnd())
              Advance();
          }
          else
          {
            AddToken(TokenType.SLASH);
          }
          break;
        case ' ':
        case '\r':
        case '\t': break;
        case '\n': Line++; break;
        case '"': StringLiteral(); break;
        default:
          if (IsDigit(c))
            NumberLiteral();
          else if (IsAlpha(c))
            Identifier();
          else
            Error.Log(Line, string.Format("Unexpected Char `{0}`", c));
          break;
      }
    }

    private void Identifier()
    {
      while (IsAlphaNumeric(Peek())) Advance();

      var text = Source[Start..Current];
      var type = TokenType.IDENTIFIER;

      if (Keywords.ContainsKey(text))
        type = Keywords[text];

      AddToken(type);
    }

    private static bool IsAlpha(char c)
    {
      return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    private static bool IsAlphaNumeric(char c)
    {
      return IsAlpha(c) || IsDigit(c);
    }

    private void NumberLiteral()
    {
      while (IsDigit(Peek())) Advance();

      if (Peek() == '.' && IsDigit(PeekNext()))
      {
        Advance();
        while (IsDigit(Peek())) Advance();
      }

      AddToken(TokenType.NUMBER, Double.Parse(Source[Start..Current]));
    }

    private static bool IsDigit(char c)
    {
      return c >= '0' && c <= '9';
    }

    private void StringLiteral()
    {
      while (Peek() != '"' && !IsAtEnd())
      {
        if (Peek() == '\n') Line++;
        Advance();
      }

      if (IsAtEnd())
      {
        Error.Log(Line, "Unterminated string literal");
        return;
      }

      // Consume the RHS "
      Advance();

      var value = Source[(Start + 1)..(Current - 1)];
      AddToken(TokenType.STRING, value);
    }

    private char PeekNext()
    {
      if (Current + 1 >= Source.Length)
        return '0';

      return Source[Current + 1];
    }
    private char Peek()
    {
      if (IsAtEnd())
        return '\0';

      return Source[Current];
    }

    private bool CanMatch(char expected)
    {
      if (IsAtEnd())
        return false;

      if (Source[Current] != expected)
        return false;

      Current++;
      return true;
    }

    private char Advance()
    {
      return Source[Current++];
    }

    private void AddToken(TokenType type)
    {
      AddToken(type, null);
    }

    private void AddToken(TokenType type, object literal)
    {
      var text = Source[Start..Current];
      Tokens.Add(new Token(type, text, literal, Line));
    }
  }
}