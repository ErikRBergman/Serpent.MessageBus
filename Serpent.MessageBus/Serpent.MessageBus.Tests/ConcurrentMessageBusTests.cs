namespace Serpent.MessageBus.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;

    public class ConcurrentMessageBusTests
    {
        [Fact]
        public async Task TestConcurrentMessageBusNormal()
        {
            var messageType1Bus = new Bus<MessageType1>();

            var type1Received = new List<MessageType1>();

            messageType1Bus.SubscribeSimple(
                message =>
                    {
                        type1Received.Add(message);
                        return Task.CompletedTask;
                    });

            const string Text = "Test";

            await messageType1Bus.PublishAsync(new MessageType1(Text));
            Assert.Single(type1Received);

            Assert.Equal(Text, type1Received.First().Name);
        }

        [Fact]
        public async Task TestConcurrentMessageBusPatternMatching()
        {
            // If you would rather have a single message bus for your entire application, that can be done in this way
            var bus = new Bus<MessageBase>();

            var type1Received = new List<MessageType1>();
            var type2Received = new List<MessageType2>();

            bus.SubscribeSimple(
                message =>
                    {
                        switch (message)
                        {
                            case MessageType1 msg:
                                type1Received.Add(msg);
                                break;

                            case MessageType2 msg:
                                type2Received.Add(msg);
                                break;
                        }

                        return Task.CompletedTask;
                    });

            await bus.PublishAsync(new MessageType1("John Yolo"));

            Assert.True(type1Received.Count == 1);
            Assert.True(type2Received.Count == 0);

            await bus.PublishAsync(new MessageType2("John Yolo"));

            Assert.True(type1Received.Count == 1);
            Assert.True(type2Received.Count == 1);
        }

        private class MessageBase
        {
        }

        // Messages should be immutable like this one
        private class MessageType1 : MessageBase
        {
            public MessageType1(string name)
            {
                this.Name = name;
            }

            public string Name { get; }
        }

        private class MessageType2 : MessageBase
        {
            public MessageType2(string name)
            {
                this.Name = name;
            }

            public string Name { get; set; }
        }
    }
}