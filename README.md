# Serpent.MessageBus

|Branch|.NET Core release build|Better code|
|:--:|:--:|:--:|
|master|[![Build Status](https://travis-ci.org/ErikRBergman/Serpent.MessageBus.svg?branch=master)](https://travis-ci.org/ErikRBergman/Serpent.MessageBus)|[![BCH compliance](https://bettercodehub.com/edge/badge/ErikRBergman/Serpent.MessageBus?branch=master)](https://bettercodehub.com/)|

Feel free to fork the project, make changes and send pull requests, report errors, suggestions, ideas, ask questions etc.

## Introduction

Serpent.MessageBus is an asynchronous message bus. Messages are dispatched through .NET Task Parallel Library (TPL), which is part of .NET. Serpent.MessageBus is built on .NET Standard 2.0, which means you can use it on any .NET runtime that supports .NET Standard 2.0, for example .NET Framework 4.6.1 (with some tricks), .NET Framework 4.7.1, .NET Core 2.0, Mono 5.4, Xamarin iOS 10.14 and Xamarin Android 8.0.

All examples should be available more or less identical in the Serpent.MessageBus.Examples project. Clone the repository to try them out or check them out here on GitHub.

## Why I use a message bus

Why would I use Serpent.MessageBus or any message bus in my application instead using normal method calls?
Well, I can come up with a few reasons.

* Loose coupling - Publisher and subscribers know nothing about each other. As long as they know about the bus and how to interpret the messages. Subscribers aand publishers can be changed, added or replaced witout affecting each other.
* A simple way to add cross cutting concerns (like retry, parallelism, thread synchronization, weak references) to your code.
* Concurrency and parallelization made easy - By adding a single line of code (`.Concurrent(16)`), you can parallelize your work on the .NET thread pool
* Reusability - Smaller components with a defined contract can more easily be reused
* Flexibility and out of the box functionality. When you have created your message handler, you can add quite some out-of-the-box functionality to it without modifying the message handler. Throttling, Exception handling, Retry, Duplicate message elimination, to name a few.

## How to install

Using Visual Studio, open the NuGet client for your project and find `Serpent.MessageBus`.

or

Open the `Package Manager Console` and type:

`install-package Serpent.MessageBus`

To start using the bus, add

```csharp
using Serpent.MessageBus;
```

## Example

This example is available in the examples project as MainExample:

```csharp
using Serpent.MessageBus;

internal class ExampleMessage
{
    public string Id { get; set; }
}

public class Program
{
    public static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
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
                    Id = "Message 1"
                });


        // Unsubscribe - the subscription implements IDisposable. Calling Dispose() unsubscribes
        subscription.Dispose();
        asynchronousSubscription.Dispose();
    }
}
```

## How to publish messages

Consider this message bus subscription in search of hotdogs (example Hotdogs)

```csharp
// The message/model class
public class FileToRead
{
    public string Filename { get; set; }
}

// Subscribe to the default FileToRead message bus
Use<FileToRead>
    .Bus
    .SubscribeSimple(
    async message =>
        {
            Console.WriteLine($"Looking for hotdogs in {message.Filename}");

            using (var fileStream = System.IO.File.OpenText(message.Filename))
            {
                while (fileStream.EndOfStream == false)
                {
                    var line = await fileStream.ReadLineAsync();
                    if (line.IndexOf(
                        "hotdog",
                        StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        Console.WriteLine("Oh! I love hotdogs! " + line);
                    }
                }
            }

            Console.WriteLine($"Looking for hotdogs in {message.Filename} done");
        });

// Publish
await Use<FileToRead>.Bus.PublishAsync(
    new FileToRead()
        {
            Filename = "c:\\temp\\hotdata.txt"
        });

Console.WriteLine("All done!");
```

Since `PublishAsync` is awaited, execution of `Console.WriteLine("All done!")` comes after the subscription code, looking for hotdogs, is done.

Example output when hotdata.txt contains the lines "Hotdogs rule!" and "Hotdog is the new burger!":

```
Looking for hotdogs in c:\temp\hotdata.txt
Oh! I love hotdogs! Hotdogs rule!
Oh! I love hotdogs! Hotdog is the new burger!
Looking for hotdogs in c:\temp\hotdata.txt done
All done
```

### But what if we don't want to wait for all subscription code to execute?

You can use the `Publish` method or create a bus that never waits for ! This will execute all synchronous code but never wait for asynchronous code to complete.

```csharp
// Publish
Use<FileToRead>.Bus.Publish(
    new FileToRead()
        {
            Filename = "c:\\temp\\hotdata.txt"
        });
```

Output if hotdata.txt contains the lines "Hotdogs rule!" and "Hotdog is the new burger!":

```text
Looking for hotdogs in c:\temp\hotdata.txt
All done
Oh! I love hotdogs! Hotdogs rule!
Oh! I love hotdogs! Hotdogs is the new burger!
Looking for hotdogs in c:\temp\hotdata.txt done
```

The result you get may be slightly different since Console.WriteLine sequence is not guaranteed.

Since "Looking for hotdogs in c:\temp\hotdata.txt" is synchronous, it's executed but as soon as we start reading the file, the operation becomes asynchronous and `Publish` returns.

## How to subscribe

A few examples of how you subscribe. These examples are found in the "SubscribeExample".

```csharp
// Add a synchronous subscriber
var subscription = bus.SubscribeSimple(message => Console.WriteLine(message.Id));

// Add an asynchronous subscriber
var asynchronousSubscription = bus
    .SubscribeSimple(async message =>
        {
            await SomeMethodAsync();
            Console.WriteLine(message.Id);
        });

...
...

// The method that gets called for each published message
private Task SomeMethodAsync(ExampleMessage message, CancellationToken token)
{
    Console.WriteLine(message.Id);
}

// An asynchronous message method subscription
var methodSubscription = bus.Subscribe(this.SomeMethodAsync);

```

Note! The message bus works internally fully with TPL and if you need asynchronous code (like I/O), use the overload returning a `Task` or use one of the subscription chain asynchronous overloads (example soon). For synchronous calls, like logging (to a logger that is already asynchronous), you can create a synchronous subscription.

### Configuring a Serpent.Chain subscription

First some simple examples

```csharp
// Remember to use "using Serpent.Chain"
using Serpent.Chain;

// Synchronous - identical to the SubscribeSimple synchronous example
var chainedSynchronousSubscription = bus.Subscribe(b => b.Handler(message => Console.WriteLine(message.Id)));

// Asynchronous - identical to the SubscribeSimple asynchronous example
var chainedAsynchronousSubscription = bus.Subscribe(b => b.Handler(async message =>
    {
        await SomeMethodAsync();
        Console.WriteLine(message.Id);
    }));

// Asynchronous with cancellation token
var chainedAsynchronousTokenSubscription = bus
    .Subscribe(
        b => b
            .Handler(
                async (message, cancellationToken) =>
    {
        await SomeMethodAsync(cancellationToken);
        Console.WriteLine(message.Id);
    }));

// Asynchronous subscription having a method handling messages
var chainedAsyncMethodSub = bus.Subscribe(b => b.Handler(SomeMethodAsync));
...
private static Task SomeMethodAsync(ExampleMessage message, CancellationToken token)
{
    // This is where the message is handled
}
```

Let''s add some Serpent.Chain functionality, shall we?

```csharp
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
```

The example above, we are sending email messages. `MailRecipient` instances published to the bus will be handled by our subscription.

We only want to send messages having NewsletterId greater than 1 and a receipient address:

```csharp
.Where(
    msg =>
        msg.NewsletterId > 1
        && !string.IsNullOrWhiteSpace(msg.EmailAddress))
```

Retry will kick in if an exception is thrown further down the chain, for example in the message handler. Make up to 3 attempts to send each message. Wait 30 seconds between each retry. If an attempt fails, invoke a method. Retry has more options than the ones shown here.

```csharp
.Retry(
    r => r
        .MaximumNumberOfAttempts(3)
        .RetryDelay(TimeSpan.FromSeconds(30))
        .OnFail((msg, exception, attempt, maxAttempts) =>
            {
                Console.WriteLine($"Sending to {msg.EmailAddress} failed: {exception.Message}. Attempt {attempt}/{maxAttempts}");
            }))
```

Send up to 20 messages to the server at once.

```csharp
.Concurrent(20)
```

For a full specification of the chain functionality, please check the [Serpent.chain](https://github.com/ErikRBergman/Serpent.chain) documentation.
Summary of most of chain functionality:

* `.Append()` - Append a new message for each message. Like LINQ `.Append()`.
* `.AppendMany()` - Append a range of messages based on an incoming message. Supports recursive unwrapping of trees and such.
* `.Branch()` - Split the chain into two or more parallel execution trees.
* `.BranchOut()` - Branch the MHC tree into one or more MHC trees parallel to the normal MHC tree.
* `.Cast()` - Cast each message to a specific type.
* `.Concurrent()` - Parallelize and handle X concurrent messages.
* `.ConcurrentFireAndForget()` - Parallelize and handle X concurrent messages but does not provide delivery feedback and does not pass through exceptions.
* `.Delay()` - Delay the execution of the message handler.
* `.DispatchOnTaskScheduler()` - Have the messages dispatched on a new Task on a specified Task Scheduler. For example, to have all messages handled by the UI thread.
* `.DispatchOnCurrentContext()` - Have the messages dispatched to a new Task on the current Task Scheduler. For example, to have all messages handled by the UI thread.
* `.Distinct()` - Only pass through unique messages based on key.
* `.Exception()` - Handle exceptions not handled by the message handler.
* `.Filter()` - Execute a method before and after the execution of the message handler. Can also filter messages to stop the message from being processed further.
* `.FireAndForget()` - Spawn a new Task to execute each message (and prevent awaiting and exceptions to pass through).
* `.First()` - Only let 1 message pass through. Optionally based by a predicate (which is the same as `.Where().First()`).
* `.LimitedThroughput()` - Limit the throughput to X messages per period. For example, 100 messages per second. Or 10 messages per 100 ms.
* `.LimitedThroughputFireAndForget()` - Same as `.LimitedThroughput()` but break the feedback chain.
* `.NoDuplicates()` - Drop all duplicate messages by key. Duplicate messages are dropped.
* `.OfType()` - Only pass on messages of a certain type. 
* `.Prepend()` - Prepend a message for each message handled. Like LINQ `.Prepend()`.
* `.Retry()` - Retry after TimeSpan, X times to deliver a message if the message handler fails (throws an exception)
* `.Select()` - Change message message type for the remaining message handler chain. Like LINQ `.Select()`.
* `.SelectMany()` - Change message message type for the remaining message handler chain and extract messages from an enumerable. Like LINQ `.SelectMany()`.
* `.Semaphore()` - Limit the number of concurrent messages being handled by this subscription. `.Semaphore()` can limit the number of concurrent messages by key as well.
* `.Skip()` - Skip the first X messages. Like LINQ `.Skip()`.
* `.SkipWhile()` - Skip messages as long as the predicate succeeds. Like LINQ `.SkipWhile()`.
* `.SoftFireAndForget()` - Executes the synchronous parts of the next MHCD or Handler, synchronous but everything asynchronous is executed without feedback. 
* `.Take()` - Only let X messages pass through. Like LINQ `.Take()`.
* `.TakeWhile()` - Only let messages pass through as long as a predicate succeeds. The same as `.Where().Take()`. Like LINQ `.TakeWhere()`.
* `.WeakReference()` - Keeps a weak reference to the message handler and unsubscribes when the message handler has been reclaimed by GC
* `.Where()` - Filter messages based on predicate. Like LINQ `.Where()`

### Unsubscribe

`Subscribe()` returns an `IMessageBusSubscription` reference which inherits from `IDisposable`. To unsubscribe, call `IMessageBusSubscription.Dispose()`.

See the Unsubscribe example.

```csharp
var subscription = bus
    .Subscribe(b => b
        .Handler(async message =>
            {
                await this.SomeMethodAsync();
                Console.WriteLine(message.Id);
            }));
...

subscription.Dispose();
```

If you are using for example a MVVM framework that manages the lifetime of your subscribers, check out the readme section about weak references.

### Using `.Factory()` to instantiate a handler for each message

Note! The handler in this example implements IDisposable, but it is not a requirement. When using a factory to instantiate an IDisposable type, the type is automatically disposed when the message has been handled (unless you specify neverDispose:true).

#### `.Factory()` example

```csharp
internal class ReadmeFactoryHandler : IMessageHandler<ExampleMessage>, IDisposable
{
    public void Dispose()
    {
        // If the type implements IDisposable.
        // The Dispose method is called as soon as the HandleMessageAsync is done
    }

    public async Task HandleMessageAsync(ExampleMessage message, CancellationToken token)
    {
        // Do something with the message
    }
}

internal class ReadmeFactoryHandlerSetup
{
    public void SetupSubscription(IMessageBusSubscriptions<ExampleMessage> bus)
    {
        bus
            .Subscribe()
            .Factory(() => new ReadmeFactoryHandler());
    }
}
```

### Publishing

You have a selection of options to customize the way messages are delivered. You can customize the way messages are published to the subscribers, and you can customize the way the subscribers handle the messages.
Customizing publishing affects all messages being published to a bus, while customizing a subscription only affects that subscription.

Use custom subscriptions before custom publishing, since it it will not affect as much 

#### Customizing the bus publisher chain

You can configure the bus using the same Serpent.Chain functionality you use to configure subscriptions.

Use the `.UseSubscriptionChain()` extension method on `BusOptions<TMessageType>` to decorate the dispatch message handler chain:

##### Overloads

```csharp
// Configures a chain without specifying a handler
.UseSubscriptionChain<TMessageType>(Action<MessageHandlerChainBuilder<MessageAndHandler<TMessageType>>> configureMessageHandlerChain);

// Configures a chain specifying a handler
.UseSubscriptionChain<TMessageType>(Action<MessageHandlerChainBuilder<MessageAndHandler<TMessageType>>, Func<MessageAndHandler<TMessageType>, CancellationToken, Task>> configureMessageHandlerChain);
```

* `configureMessageHandlerChain` - The method called to configure the bus options.

The SubscriptionChain receives a message of type `MessageAndHandler<TMessageType>` for each subscription:

```csharp
public struct MessageAndHandler<TMessageType>
{
    public TMessageType Message { get; }
    public Func<TMessageType, CancellationToken, Task> MessageHandler { get; }
}
```

* `Message` - the message.
* `MessageHandler` - the subscription message handler method

##### `.UseSubscriptionChain()` example

```csharp
var bus = new Bus<TestMessage>(
    options => options.UseSubscriptionChain(
        chain => chain
            .Concurrent(16)
            .Filter(
                messageBeforeHandler =>
                    {
                        Console.WriteLine("Before the message is invoked");
                    },
                messageAfterHandler =>
                    {
                        Console.WriteLine("After the message was invoked")
                    });
            ));
```

Make sure you call the handler method at the end of the chain or your subscribers will not be called.

### Weak references

There are times when you are unable to control the lifetime of your subscribers, for example using an MVVM framework. Normally, the message bus (and Serpent.Chain) will hold a strong reference to subscribers, which will keep them from being garbage collected, unless your (MVVM) framework has a mechanism for disposing the objects. This in turn can cause your application to add more and more subscribers, acting on application events, which can turn into a memory leak and performance nightmare. The solution is weak references.

Weak references are basically references that will not prevent the instance from being garbage collected. Weak references in Serpent.MessageBus will unsubscribe when the message handler is garbage collected. Use `.WeakReference()` instead of `.Handler()`.
It also requires the message handler to implement `IMessageHandler<TMessageType>`.

Example:

```csharp
public class Handler: IMessageHandler<MyMessage>
{
    public Handler()
    {
        Use<MyMessage>.Bus.Subscribe(b => b.WeakReference(this));
    }

    public override Task HandleMessageAsync(MyMessage message, CancellationTokent token)
    {
        return Task.CompletedTask;
    }
}

```

When a message is published to the bus and the handler is garbage collected, the subscription is unsubscribed. There is also a weak reference subscription garbage collector, running once every 60 seconds, checking and unsubscribing all garbage collected message handles referenced by `.WeakReference()`.

### ASP.NET Core

Check out the ASP.NET Core example project

```csharp
using Serpent.MessageBus;
using Serpent.MessageBus.Extras;

public class Startup
{
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        // Register generic message bus singletons
        services.AddSingleton(typeof(IMessageBus<>), typeof(Bus<>));

        // These two are required if you want to be able to resolve IMessageBusPublisher<> and IMessageBusSubscriptions
        services.AddSingleton(typeof(IMessageBusPublisher<>), typeof(PublisherBridge<>));
        services.AddSingleton(typeof(IMessageBusSubscriptions<>), typeof(SubscriptionsBridge<>));

        // To resolve based on service type
        services.AddSingleton<ReadmeService>();

        // To resolve based on message handler
        services.AddSingleton<ReadmeService, IMessageHandler<ReadmeMessage>();
    }
}

```

#### Resolving message handlers using dependency injection

Use your favorite dependency injection framework tos produce the handler instance.

##### Resolving with ASP.NET Core dependency injection

Registering the bus and the sample service

```csharp
using Serpent.MessageBus;
using Serpent.MessageBus.Extras;

public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // Register message bus for all types
    services.AddSingleton(typeof(IMessageBus<>), typeof(Bus<>));

    // These two are required if you want to be able to resolve IMessageBusPublisher<> and IMessageBusSubscriptions
    services.AddSingleton(typeof(IMessageBusPublisher<>), typeof(PublisherBridge<>));
    services.AddSingleton(typeof(IMessageBusSubscriptions<>), typeof(SubscriptionsBridge<>));

    // Register the ReadmeService based on service type
    services.AddSingleton<ReadmeService>();

    // *OR* register the service as IMessageHandler<ReadmeMessage>
    services.AddSingleton<ReadmeService, IMessageHandler<ReadmeMessage>>();
}
```

Resolving a handler using the factory method

```csharp
public void SetupSubscriptions(IMesssageBusSubscriptions<ReadmeMessage> bus, IServiceProvider services)
{
    // resolve by service type
    bus
        .Subscribe(b => b.Factory(() => services.GetService<ReadmeService>()));

    // resolve by IMessageHandler interface
    bus
        .Subscribe(
            b => b.Factory(
             () => services.GetService<IMessageHandler<ReadmeMessage>>()));
}
```

##### MessageBus and Autofac

Register the generic bus and the sample handler

```csharp
public void ConfigureServices(IRegistrationBuilder builder)
{
    // Register message busses for all types
    builder
        .RegisterGeneric(typeof(Bus<>))
        .As(typeof(IMessageBusPublisher<>))
        .As(typeof(IMessageBusSubscriptions<>))
            .SingleInstance();

    // Register the ReadmeService based on service type and IMessageHandler<>
    builder
        .RegisterType<ReadmeService>()
            .As<ReadmeService>()
            .As<IMessageHandler<ReadmeService>>();
}
```

Resolving a handler by using the factory method

```csharp
public void SetupSubscriptions(IMesssageBusSubscriptions<ReadmeMessage> bus, IComponentContext services)
{
    // Resolve using service type
    bus.Subscribe(
        b => b.Factory(
            () => services.Resolve<ReadmeService>()));

    // Using IMessageHandler<>
    bus.Subscribe(
        b => b.Factory(
            () => services.Resolve<IMessageHandler<ReadmeMessage>>()));
}
```
