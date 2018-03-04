namespace Serpent.MessageBus.Examples
{
    using System;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            ConsoleKeyInfo key;

            do
            {
                Console.WriteLine("Welcome to Serpent.MessageBus.Examples!");
                Console.WriteLine();
                Console.WriteLine("1. Main example");
                Console.WriteLine("2. Hotdogs example");
                Console.WriteLine("3. Weak reference example");
                Console.WriteLine();
                Console.WriteLine("Q. Quit");
                Console.WriteLine();

                key = Console.ReadKey(true);


                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        // Main example
                        await MainExample.MainExample.MainExampleAsync();
                        break;

                    case ConsoleKey.D2:
                        // In persuit of hotdogs
                        await Hotdogs.Hotdogs.HotdogsAsync();
                        break;

                    case ConsoleKey.D3:
                        // In persuit of hotdogs
                        WeakReferenceExample.WeakReferenceExample.WeakReferenceExampleMethod();
                        break;

                }
            }
            while (key.Key != ConsoleKey.Q);

        }
    }
}