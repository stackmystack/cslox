using System;

namespace cslox
{
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