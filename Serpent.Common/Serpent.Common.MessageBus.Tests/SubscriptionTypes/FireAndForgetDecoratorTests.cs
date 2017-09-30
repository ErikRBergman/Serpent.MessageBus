﻿namespace Serpent.Common.MessageBus.Tests.SubscriptionTypes
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FireAndForgetDecoratorTests
    {
        [TestMethod]
        public async Task TestFireAndForgetSubscription()
        {
            var bus = new ConcurrentMessageBus<Message1>();

            bus.Subscribe().FireAndForget()
                .Handler(
                msgz =>
                    {
                        msgz.Status = "Got it!";
                    });

            var msg = new Message1();

            await bus.PublishAsync(msg);

            await Task.Delay(200);

            Assert.AreEqual("Got it!", msg.Status);
        }

        private class Message1
        {
            public string Status { get; set; }
        }
    }
}