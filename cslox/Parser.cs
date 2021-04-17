using System;
using System.Collections.Generic;
using cslox.AST;

namespace cslox
{
    public class Parser
    {
        public class ParseError : Exception { }

        private List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Equality();
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

        private ParseError Err(Token token, string message)
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