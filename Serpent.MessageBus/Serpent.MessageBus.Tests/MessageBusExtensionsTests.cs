namespace Serpent.MessageBus.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Serpent.Chain;

    using Xunit;

    public class MessageBusExtensionsTests
    {
        [Fact]
        public void TestSingleBusMultipleTypesExtensions()
        {
            var bus = new Bus<BaseMessageType>();

            var type1Received = new List<MessageType1>();
            var type2Received = new List<MessageType2>();

            var subscription1 = bus.Subscribe(b => b.OfType<BaseMessageType, MessageType1>().Handler(
                msg =>
                    {
                        type1Received.Add(msg);
                        return Task.CompletedTask;
                    }));

            var subscription2 = bus.Subscribe(b => b.OfType<BaseMessageType, MessageType2>().Handler(
                msg =>
                    {
                        type2Received.Add(msg);
                        return Task.CompletedTask;
                    }));

            bus.PublishAsync(new MessageType1("Haj"));
            bus.PublishAsync(
                new MessageType2()
                {
                    Name = "Boj"
                });

            Assert.Single(type1Received);
            Assert.Single(type2Received);

            subscription2.Dispose();
            subscription1.Dispose();
        }

        private class BaseMessageType
        {
        }

        // Messages should be immutable like this one
        private class MessageType1 : BaseMessageType
        {
            public MessageType1(string name)
            {
                this.Name = name;
            }

            public string Name { get; }
        }

        private class MessageType2 : BaseMessageType
        {
            public string Name { get; set; }
        }
    }
}