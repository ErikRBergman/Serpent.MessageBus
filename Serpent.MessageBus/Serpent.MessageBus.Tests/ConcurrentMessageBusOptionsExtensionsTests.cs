namespace Serpent.MessageBus.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;

    using Xunit;

    public class ConcurrentMessageBusOptionsExtensionsTests
    {
        private const int DelayMultiplier = 5;

        [Fact]
        public async Task UseCustomPublisherTests()
        {
            var mypublisher = new MyPublisher();
            var bus = new Bus<int>(options => options.UseCustomPublisher(mypublisher));

            await bus.PublishAsync(55);
            await bus.PublishAsync(1);
            await bus.PublishAsync(5);

            Assert.Equal(3, mypublisher.Messages.Count);

            Assert.Equal(55, mypublisher.Messages[0]);
            Assert.Equal(1, mypublisher.Messages[1]);
            Assert.Equal(5, mypublisher.Messages[2]);
        }

        [Fact]
        public async Task UseSubscriptionChainWithHandlerTests()
        {
            // Test having message handler decorators both in the publish dispatch and the Subscription
            var bus = new Bus<TestMessage>(
                options => options.UseSubscriptionChain(
                    (chain, handler) =>
                        {
                            chain.Action(
                                a =>
                                    a.Before(message => message.Message.Log.TryAdd("Before", DateTime.Now))
                                     .Finally(message => message.Message.Log.TryAdd("After", DateTime.Now)))
                                .Handler(handler);
                        }));

            bus.Subscribe(builder => builder
                .Handler(
                    async message =>
                        {
                            await Task.Delay(DelayMultiplier * 100);
                            message.Log.TryAdd("Handler", DateTime.Now);
                            await Task.Delay(DelayMultiplier * 100);
                        }));

            var msg = new TestMessage();
            bus.Publish(msg);

            await Task.Delay(50);

            Assert.Single(msg.Log);
            Assert.True(msg.Log.ContainsKey("Before"));

            await Task.Delay(DelayMultiplier * 100);

            Assert.Equal(2, msg.Log.Count);
            Assert.True(msg.Log.ContainsKey("Before"));
            Assert.True(msg.Log.ContainsKey("Handler"));

            await Task.Delay(DelayMultiplier * 100);

            Assert.Equal(3, msg.Log.Count);
            Assert.True(msg.Log.ContainsKey("Before"));
            Assert.True(msg.Log.ContainsKey("Handler"));
            Assert.True(msg.Log.ContainsKey("After"));
        }

        [Fact]
        public async Task UseSubscriptionChainWithoutHandlerTests()
        {
            // Test having message handler decorators both in the publisher and the subscription
            var bus = new Bus<TestMessage>(
                options => options.UseSubscriptionChain(
                    chain => chain
                        .Action(c =>
                        c.Before(message => message.Message.Log.TryAdd("Before", DateTime.Now))
                         .Finally(message => message.Message.Log.TryAdd("After", DateTime.Now)))));

            bus.Subscribe(builder =>
                builder.Handler(
                    async message =>
                        {
                            await Task.Delay(DelayMultiplier * 100);
                            message.Log.TryAdd("Handler", DateTime.Now);
                            await Task.Delay(DelayMultiplier * 100);
                        }));

            var msg = new TestMessage();
            bus.Publish(msg);

            await Task.Delay(50);

            Assert.Single(msg.Log);
            Assert.True(msg.Log.ContainsKey("Before"));

            await Task.Delay(DelayMultiplier * 100);

            Assert.Equal(2, msg.Log.Count);
            Assert.True(msg.Log.ContainsKey("Before"));
            Assert.True(msg.Log.ContainsKey("Handler"));

            await Task.Delay(DelayMultiplier * 100);

            Assert.Equal(3, msg.Log.Count);
            Assert.True(msg.Log.ContainsKey("Before"));
            Assert.True(msg.Log.ContainsKey("Handler"));
            Assert.True(msg.Log.ContainsKey("After"));
        }

        private class MyPublisher : BusPublisher<int>
        {
            public List<int> Messages { get; } = new List<int>();

            public override Task PublishAsync(IEnumerable<Func<int, CancellationToken, Task>> handlers, int message, CancellationToken cancellationToken)
            {
                this.Messages.Add(message);
                return Task.CompletedTask;
            }
        }

        private class TestMessage
        {
            public ConcurrentDictionary<string, DateTime> Log { get; } = new ConcurrentDictionary<string, DateTime>();
        }
    }
}