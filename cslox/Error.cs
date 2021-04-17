using System;

namespace cslox
{
    class Error
    {
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