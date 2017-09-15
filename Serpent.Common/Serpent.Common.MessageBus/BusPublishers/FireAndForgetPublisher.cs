﻿namespace Serpent.Common.MessageBus.BusPublishers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class FireAndForgetPublisher<T> : BusPublisher<T>
    {
        private readonly BusPublisher<T> innerPublisher;

        public FireAndForgetPublisher(BusPublisher<T> innerPublisher)
        {
            this.innerPublisher = innerPublisher ?? ParallelPublisher<T>.Default;
        }

        public override Task PublishAsync(IEnumerable<ISubscription<T>> subscriptions, T message)
        {
            Task.Run(() => this.innerPublisher.PublishAsync(subscriptions, message));
            return Task.CompletedTask;
        }
    }
}