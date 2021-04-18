using System;

namespace cslox
{
    public class RuntimeError : Exception
    {
        public readonly Token Token;

        public RuntimeError(Token token, string message) : base(message)
        {
            this.Token = token;
        }

        public RuntimeError(Token token, string message, Exception inner) : base(message, inner)
        {
            this.Token = token;
        }
    }

    static class Error
    {
        public static void Log(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Report(token.Line, " at end", message);
            else
                Report(token.Line, " at '" + token.Lexeme + "'", message);
        }

        public static void Log(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("@ {0} | Error{1}: {2}", line, where, message);
        }
    }
}