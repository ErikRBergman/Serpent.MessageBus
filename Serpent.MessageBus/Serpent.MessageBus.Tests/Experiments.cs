namespace Serpent.MessageBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq.Expressions;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;

    using Xunit;

    public class Experiments
    {
        [Fact]
        //[Ignore]
        public async Task TestMethod1()
        {
            Use<int>.Bus.SubscribeSimple(msg => Console.WriteLine(msg));


            await Task.Delay(1);
        }
    }


    internal class ExampleMessage
    {
        public string Id { get; set; }
    }

    public class MailRecipient
    {
        public string EmailAddress { get; set; }

        public int NewsletterId { get; set; }
    }

    public class FileToRead
    {
        public string Filename { get; set; }
    }

    public class Program
    {
        public static async Task MainAsync()
        {
            Use<FileToRead>.Bus.SubscribeSimple(
                async message =>
                    {
                        Console.WriteLine($"Looking for hotdogs in {message.Filename}");

                        using (var fileStream = System.IO.File.OpenText(message.Filename))
                        {
                            while (fileStream.EndOfStream == false)
                            {
                                var line = await fileStream.ReadLineAsync();
                                if (line.IndexOf("hotdogs", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    Console.WriteLine("Oh! I love hotdogs! " + line);
                                }
                            }
                        }

                        Console.WriteLine($"Looking for hotdogs in {message.Filename} done");
                    });

            await Use<FileToRead>.Bus.PublishAsync(
                new FileToRead()
                    {
                        Filename = "c:\\temp\\hotdata.txt"
                    });

            var smtpClient = new SmtpClient();
            
            Use<MailRecipient>
                .Bus
                .Subscribe(b => b
                    // Only handle messages with NewsletterId > 1 and receipient
                    .Where(msg => msg.NewsletterId > 1 && !string.IsNullOrWhiteSpace(msg.EmailAddress))
                    // Try up to a total of 3 times if the handler fails
                    .Retry(
                        r => r
                            .MaximumNumberOfAttempts(3)
                            .RetryDelay(TimeSpan.FromSeconds(30))
                            .OnFail((msg, exception, attempt, maxAttempts) =>
                                {
                                    Console.WriteLine($"Sending to {msg.EmailAddress} failed: {exception.Message}. Attempt {attempt}/{maxAttempts}");
                                }))
                    // Send up to 20 messages concurrently to the smtp server
                    .Concurrent(20)
                    .Handler(async message =>
                        {
                            await smtpClient.SendMailAsync(
                                new MailMessage(
                                    "noreply@mynewsletter.test",
                                    message.EmailAddress,
                                    "Your daily news",
                                    "This is the news letter content"));
                        }));



            // Create a message bus
            var bus = new Bus<ExampleMessage>();

            bus.Publish();

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

            // An asynchronous message subscription
            var methodSubscription = bus.Subscribe(SomeMethodAsync);
            
            var chainedSynchronousSubscription = bus.Subscribe(b => b.Handler(message => Console.WriteLine(message.Id)));
            var chainedAsynchronousSubscription = bus.Subscribe(b => b.Handler(async message =>
                {
                    await SomeMethodAsync();
                    Console.WriteLine(message.Id);
                }));
            var chainedAsynchronousTokenSubscription = bus.Subscribe(b => b.Handler(async (message, cancellationToken) =>
                {
                    await SomeMethodAsync(cancellationToken);
                    Console.WriteLine(message.Id);
                }));
            var chainedAsyncMethodSub = bus.Subscribe(b => b.Handler(SomeMethodAsync));

            // Publish a message to the bus
            await bus.PublishAsync(
                new ExampleMessage
                {
                    Id = "Message 1"
                });
        }

        private static Task SomeMethodAsync()
        {
            throw new NotImplementedException();
        }

        private static Task SomeMethodAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private static Task SomeMethodAsync(ExampleMessage message, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}