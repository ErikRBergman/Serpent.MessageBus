// ReSharper disable once CheckNamespace

namespace Serpent.MessageBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;
    using Serpent.Chain.Notification;
    using Serpent.MessageBus.BusPublishers;
    using Serpent.MessageBus.Models;

    /// <summary>
    ///     Extensions for parallel message handler chain publisher
    /// </summary>
    public static class ParallelMessageHandlerChainPublisherExtensions
    {
        /// <summary>
        ///     Sets up a message handler chain for the bus publisher
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <param name="configureMessageHandlerChain">The action called to setup the message handler chain</param>
        /// <returns>Bus options</returns>
        public static BusOptions<TMessageType> UseSubscriptionChain<TMessageType>(
            this BusOptions<TMessageType> options,
            Action<ChainBuilder<MessageAndHandler<TMessageType>>, Func<MessageAndHandler<TMessageType>, CancellationToken, Task>> configureMessageHandlerChain)
        {
            var builder = new ChainBuilder<MessageAndHandler<TMessageType>>();
            configureMessageHandlerChain(builder, PublishToSubscription.PublishAsync);

            return UseSubscriptionChain(options, builder);
        }

        /// <summary>
        ///     Sets up a message handler chain for the bus publisher
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <param name="configureMessageHandlerChain">The action called to setup the message handler chain</param>
        /// <returns>Bus options</returns>
        public static BusOptions<TMessageType> UseSubscriptionChain<TMessageType>(
            this BusOptions<TMessageType> options,
            Action<ChainBuilder<MessageAndHandler<TMessageType>>> configureMessageHandlerChain)
        {
            var builder = new ChainBuilder<MessageAndHandler<TMessageType>>();
            configureMessageHandlerChain(builder);

            return UseSubscriptionChain(options, builder);
        }

        private static BusOptions<TMessageType> UseSubscriptionChain<TMessageType>(
            BusOptions<TMessageType> options,
            ChainBuilder<MessageAndHandler<TMessageType>> builder)
        {
            if (builder.HasHandler == false)
            {
                builder.Handler(PublishToSubscription.PublishAsync);
            }

            var subscriptionNotification = new ChainBuilderNotifier();
            var services = new ChainBuilderSetupServices(subscriptionNotification);
            var chainFunc = builder.BuildFunc(services);
            var newChain = new Chain<MessageAndHandler<TMessageType>>(chainFunc);
            subscriptionNotification.Notify(newChain);

            options.UseCustomPublisher(new ParallelMessageHandlerChainPublisher<TMessageType>(chainFunc));
            return options;
        }
    }
}