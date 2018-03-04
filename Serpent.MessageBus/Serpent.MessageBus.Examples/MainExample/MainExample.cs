namespace Serpent.MessageBus.Examples.MainExample
{
    using System;
    using System.Threading.Tasks;

    using Serpent.MessageBus.Examples.Extensions;

    public class MainExample
    {
        public static async Task MainExampleAsync()
        {
            ColorConsole.WriteLine();
            ColorConsole.WriteLine(ConsoleColor.White, "** MainExample **");

            // Create a message bus
            var bus = new Bus<ExampleMessage>();

            // "Use<ExampleMessage>.Bus" is a shortcut to a singleton bus instance for ExampleMessage
            // var bus = Use<ExampleMessage>.Bus;

            // Add a synchronous subscriber
            var subscription = bus
                .SubscribeSimple(message => Console.WriteLine(message.Id));

            // Add an asynchronous subscriber
            var asynchronousSubscription = bus
                .SubscribeSimple(async message =>
                    {
                        await SomeMethodAsync();
                        Console.WriteLine(message.Id);
                    });

            // Publish a message to the bus awaiting the message to pass through
            await bus.PublishAsync(
                new ExampleMessage
                    {
                        Id = "Message 1"
                    });

            // Publish a message but do not wait for any asynchronous operations
            bus.Publish(
                new ExampleMessage
                    {
                        Id = "Message 2"
                    });


            // Unsubscribe - the subscription implements IDisposable. Calling Dispose() unsubscribes
            subscription.Dispose();
            asynchronousSubscription.Dispose();

            await Task.Delay(TimeSpan.FromMilliseconds(200));

            ColorConsole.WriteLine();
            ColorConsole.WriteLine(ConsoleColor.White, "** End of MainExample **");

        }

        private static Task SomeMethodAsync()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}
