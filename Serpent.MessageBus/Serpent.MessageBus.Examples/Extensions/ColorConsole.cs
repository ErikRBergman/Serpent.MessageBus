namespace Serpent.MessageBus.Examples.Extensions
{
    using System;

    public static class ColorConsole
    {
        public static void WriteLine(ConsoleColor color, string message, params object[] arg)
        {
            var previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message, arg);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        public static void WriteLine(string message, params object[] arg)
        {
            Console.WriteLine(message, arg);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }
    }
}