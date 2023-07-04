using System;
using System.Threading;

namespace BMOnline.Common
{
    public static class Log
    {
        private static readonly SemaphoreSlim logSemaphore = new SemaphoreSlim(1);

        private static void PrintMessage(string message, ConsoleColor foregroundColour)
        {
            if (logSemaphore.Wait(1000))
            {
                Console.ResetColor();
                Console.Write('[');
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Online");
                Console.ResetColor();
                Console.Write("] ");

                Console.ForegroundColor = foregroundColour;
                Console.WriteLine(message);
                Console.ResetColor();
                logSemaphore.Release();
            }
        }

        public static void Config(string message) => PrintMessage(message, ConsoleColor.Blue);

        public static void Info(string message) => PrintMessage(message, ConsoleColor.White);

        public static void Success(string message) => PrintMessage(message, ConsoleColor.Green);

        public static void Warning(string message) => PrintMessage(message, ConsoleColor.DarkYellow);

        public static void Error(string message) => PrintMessage(message, ConsoleColor.Red);
    }
}
