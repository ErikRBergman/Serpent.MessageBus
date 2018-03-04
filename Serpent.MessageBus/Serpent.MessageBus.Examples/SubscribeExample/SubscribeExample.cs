namespace Serpent.MessageBus.Examples.SubscribeExample
{
    using System;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;

    public class SubscribeExample
    {
        public static Task MessageHandlerAsync(ExampleMessage message, CancellationToken token)
        {
            Console.WriteLine(message.Id);
            return Task.CompletedTask;
        }

        public static Task SomeMethodAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public static void Subscribe()
        {
            var bus = new Bus<ExampleMessage>();

            // Add a synchronous subscriber
            var subscription = bus.SubscribeSimple(message => Console.WriteLine(message.Id));

            // Add an asynchronous subscriber
            var asynchronousSubscription = bus.SubscribeSimple(
                async message =>
                    {
                        await SomeMethodAsync();
                        Console.WriteLine(message.Id);
                    });

            // An asynchronous message subscription
            var methodSubscription = bus.Subscribe(MessageHandlerAsync);

            // Serpent.Chain functionality

            // Synchronous - identical to the SubscribeSimple synchronous example
            var chainedSynchronousSubscription = bus.Subscribe(b => b.Handler(message => Console.WriteLine(message.Id)));

            // Asynchronous - identical to the SubscribeSimple asynchronous example
            var chainedAsynchronousSubscription = bus.Subscribe(
                b => b.Handler(
                    async message =>
                        {
                            await SomeMethodAsync();
                            Console.WriteLine(message.Id);
                        }));

            // Asynchronous with cancellation token
            var chainedAsynchronousTokenSubscription = bus.Subscribe(
                b => b.Handler(
                    async (message, cancellationToken) =>
                        {
                            await SomeMethodAsync(cancellationToken);
                            Console.WriteLine(message.Id);
                        }));

            // Asynchronous subscription having a method handling messages
            var chainedAsyncMethodSub = bus.Subscribe(b => b.Handler(MessageHandlerAsync));

            // Send mail example
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


        }

        private static Task SomeMethodAsync()
        {
            return Task.CompletedTask;
        }

        public class ExampleMessage
        {
            public string Id { get; set; }
        }

        public class MailRecipient
        {
            public string EmailAddress { get; set; }

            public int NewsletterId { get; set; }
        }
    }
}