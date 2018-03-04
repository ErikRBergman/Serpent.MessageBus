namespace Serpent.MessageBus.Tests.Decorators
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;
    using Serpent.Chain.Interfaces;

    using Xunit;

    public class WeakReferenceDecoratorTests
    {
        [Fact]
        public async Task WeakReferenceDecoratorTest()
        {
            // new int[] { 1 }.SelectMany()
            var bus = new Bus<int>();

            Assert.Equal(0, bus.SubscriberCount);

            // Strong references
            bus.Subscribe(b => b.Concurrent(5).Retry(2, TimeSpan.FromSeconds(10)).Handler(new WeakReferenceHandler()));

            Assert.Equal(1, bus.SubscriberCount);
            GC.Collect(2, GCCollectionMode.Forced);

            await bus.PublishAsync();

            Assert.Equal(1, bus.SubscriberCount);

            // Now weak reference
            bus.Subscribe(b => b.WeakReference(new WeakReferenceHandler()));

            Assert.Equal(2, bus.SubscriberCount);
            GC.Collect(2, GCCollectionMode.Forced);
            await bus.PublishAsync();

            Assert.Equal(1, bus.SubscriberCount);
        }

        private class WeakReferenceHandler : IMessageHandler<int>
        {
            public Task HandleMessageAsync(int message, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}