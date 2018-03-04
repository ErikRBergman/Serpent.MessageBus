namespace Serpent.MessageBus.Examples.Hotdogs
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Serpent.MessageBus.Examples.Extensions;

    public class Hotdogs
    {
        public static async Task HotdogsAsync()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine(ConsoleColor.White, "** Hotdogs example **");

            IMessageBusSubscription subscription = null;

            try
            {
                // Subscribe to the default FileToRead message bus
                subscription = Use<FileToRead>.Bus.SubscribeSimple(
                    async message =>
                        {
                            ColorConsole.WriteLine($"Looking for hotdogs in {Path.GetFileName(message.Filename)}...");

                            using (var fileStream = File.OpenText(message.Filename))
                            {
                                while (fileStream.EndOfStream == false)
                                {
                                    var line = await fileStream.ReadLineAsync();
                                    if (line.IndexOf("hotdog", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    {
                                        Console.WriteLine("* Oh! I love hotdogs! " + line);
                                    }
                                }
                            }

                            Console.WriteLine($"Looking for hotdogs in {Path.GetFileName(message.Filename)} done...");
                        });

                var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hotdogsexample\\hotdata.txt");

                Console.WriteLine();
                ColorConsole.WriteLine(ConsoleColor.White, "*** await PublishAsync ***");

                // Publish asynchronous
                await Use<FileToRead>.Bus.PublishAsync(
                    new FileToRead
                    {
                        Filename = filePath
                    });

                Console.WriteLine("await PublishAsync done!");
                Console.WriteLine();

                Console.WriteLine("*** Publish ***");

                // Publish asynchronous
                Use<FileToRead>.Bus.Publish(
                    new FileToRead
                    {
                        Filename = filePath
                    });

                Console.WriteLine("Publish done!");

                // Just to make sure the message is done before continuing
                await Task.Delay(200);

                ColorConsole.WriteLine();
                ColorConsole.WriteLine(ConsoleColor.White, "** End of hotdogs example **");
            }
            finally
            {
                // Unsubscribe!
                // to prevent us from adding subscription after subscription when running the example multiple times.
                subscription?.Dispose();
            }
        }
    }
}