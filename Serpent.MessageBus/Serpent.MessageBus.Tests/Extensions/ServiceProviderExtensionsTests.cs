namespace Serpent.MessageBus.Tests.Extensions
{
    using System;
    using System.Collections.Concurrent;

    using Serpent.Chain;
    using Serpent.MessageBus.Extensions;

    using Xunit;

    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void TestServiceProviderExtensions()
        {
            var intBus = new ConcurrentMessageBus<int>();

            var serviceProvider = new MyServiceProvider().Add<int>(() => 1).Add<IMessageBusPublisher<int>>(() => intBus).Add<IMessageBusSubscriptions<int>>(() => intBus);

            var intValue = serviceProvider.GetService<int>();
            Assert.Equal(1, intValue);

            var publisher = serviceProvider.Publisher<int>();
            Assert.Equal(publisher, intBus);

            serviceProvider.GetService<ServiceProviderExtensionsTests>();

            var count = 0;
            serviceProvider.Subscribe<int>(s => s.Handler(m => count += m));

            serviceProvider.Publish(5);

            Assert.Equal(5, count);

            serviceProvider.PublishRange(new[] { 5, 5, 5 });

            Assert.Equal(20, count);
        }

        public class MyServiceProvider : IServiceProvider
        {
            public ConcurrentDictionary<Type, Func<object>> Types { get; } = new ConcurrentDictionary<Type, Func<object>>();

            public MyServiceProvider Add<T>(Func<object> factory)
            {
                this.Types.TryAdd(typeof(T), factory);
                return this;
            }

            public object GetService(Type serviceType)
            {
                if (this.Types.TryGetValue(serviceType, out var factory))
                {
                    return factory();
                }

                return null;
            }
        }
    }
}