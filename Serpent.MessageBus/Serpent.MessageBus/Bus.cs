namespace Serpent.MessageBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.MessageBus.Helpers;

    /// <summary>
    ///     The message bus
    /// </summary>
    /// <typeparam name="TMessageType">The message type</typeparam>
    public class Bus<TMessageType> : IMessageBus<TMessageType>
    {
        private readonly ExclusiveAccess<int> currentSubscriptionId = new ExclusiveAccess<int>(0);

        private readonly object subscriptionsCacheLock = new object();

        private readonly BusOptions<TMessageType> options = BusOptions<TMessageType>.Default;

        private readonly Func<IEnumerable<Func<TMessageType, CancellationToken, Task>>, TMessageType, CancellationToken, Task> publishAsyncFunc = ParallelPublisher<TMessageType>.Default.PublishAsync;

        private readonly ConcurrentQueue<int> recycledSubscriptionIds = new ConcurrentQueue<int>();

        private readonly ConcurrentDictionary<int, Func<TMessageType, CancellationToken, Task>> subscriptions = new ConcurrentDictionary<int, Func<TMessageType, CancellationToken, Task>>();

        private IEnumerable<Func<TMessageType, CancellationToken, Task>> subscriptionCache = Array.Empty<Func<TMessageType, CancellationToken, Task>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bus{TMessageType}"/> class. 
        /// </summary>
        /// <param name="options">
        /// The options
        /// </param>
        public Bus(BusOptions<TMessageType> options)
        {
            this.options = options;
            this.publishAsyncFunc = this.options.CustomPublishFunc ?? this.publishAsyncFunc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bus{TMessageType}"/> class. 
        /// </summary>
        /// <param name="optionsAction">
        /// A method that configures the message bus options
        /// </param>
        public Bus(Action<BusOptions<TMessageType>> optionsAction)
        {
            var newOptions = new BusOptions<TMessageType>();
            optionsAction(newOptions);
            this.options = newOptions;
            this.publishAsyncFunc = this.options.CustomPublishFunc ?? this.publishAsyncFunc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bus{TMessageType}"/> class. 
        /// </summary>
        public Bus()
        {
            this.publishAsyncFunc = this.options.CustomPublishFunc ?? this.publishAsyncFunc;
        }

        /// <summary>
        ///     The current number of subscribers
        /// </summary>
        public int SubscriberCount => this.subscriptions.Count;

        /// <summary>
        ///     Publishes a messages to the message bus, returning a Task that is done when the message is handled
        /// </summary>
        /// <param name="message">
        ///     The messgae to publish
        /// </param>
        /// <param name="token">
        ///     A cancellation token that can be used to cancel handling the message
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public Task PublishAsync(TMessageType message, CancellationToken token)
        {
            return this.publishAsyncFunc(this.subscriptionCache, message, token);
        }

        /// <summary>
        ///     Subscribes to the message bus with the specified method handling all messagess
        /// </summary>
        /// <param name="handlerFunc">
        ///     The handler
        /// </param>
        /// <returns>
        ///     The <see cref="IMessageBusSubscription" /> used to unsubscribe.
        /// </returns>
        public IMessageBusSubscription Subscribe(Func<TMessageType, CancellationToken, Task> handlerFunc)
        {
            var newSubscriptionId = this.GetNewSubscriptionId();

            this.subscriptions.TryAdd(newSubscriptionId, handlerFunc);

            lock (this.subscriptionsCacheLock)
            {
                this.subscriptionCache = this.subscriptions.Values;
            }

            return this.CreateSubscription(newSubscriptionId);
        }

        private BusSubscription CreateSubscription(int newSubscriptionId)
        {
            return new BusSubscription(() => this.Unsubscribe(newSubscriptionId));
        }

        private int GetNewSubscriptionId()
        {
            if (!this.recycledSubscriptionIds.TryDequeue(out var result))
            {
                result = this.currentSubscriptionId.Increment();
            }

            return result;
        }

        private void Unsubscribe(int subscriptionId)
        {
            this.currentSubscriptionId.Update(
                v =>
                    {
                        if (this.subscriptions.TryRemove(subscriptionId, out _))
                        {
                            if (subscriptionId == v)
                            {
                                --v;
                            }
                            else
                            {
                                this.recycledSubscriptionIds.Enqueue(subscriptionId);
                            }

                            lock (this.subscriptionsCacheLock)
                            {
                                this.subscriptionCache = this.subscriptions.Values;
                            }
                        }

                        return v;
                    });
        }
    }
}