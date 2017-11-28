namespace Serpent.MessageBus.Extensions
{
    using System;
    using System.Collections.Generic;

    using Serpent.Chain;

    /// <summary>
    /// The service provider extensions
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets a service of type T
        /// </summary>
        /// <typeparam name="T">The type to get</typeparam>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>An instance of T</returns>
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }

        /// <summary>
        /// Gets a IMessageBusPublisher&lt;TMessageType&gt; service from the service provider and publishes the message to it
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="serviceProvider">The service type</param>
        /// <param name="message">The message to publish</param>
        public static void Publish<TMessageType>(this IServiceProvider serviceProvider, TMessageType message)
        {
            serviceProvider.Publisher<TMessageType>().Publish(message);
        }

        /// <summary>
        /// Gets a IMessageBusPublisher&lt;TMessageType&gt; from the service provider
        /// </summary>
        /// <typeparam name="TMessageType">
        /// The message type
        /// </typeparam>
        /// <param name="serviceProvider">
        /// The service type
        /// </param>
        /// <returns>
        /// The <see cref="IMessageBusPublisher&lt;TMessageType&gt;"/>The message bus publisher for the &lt;TMessageType&gt;
        /// </returns>
        public static IMessageBusPublisher<TMessageType> Publisher<TMessageType>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IMessageBusPublisher<TMessageType>>();
        }

        /// <summary>
        /// Gets a IMessageBusPublisher&lt;TMessageType&gt; service from the service provider and publishes a range of messages to it
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="serviceProvider">The service type</param>
        /// <param name="messages">The messages to publish</param>
        public static void PublishRange<TMessageType>(this IServiceProvider serviceProvider, IEnumerable<TMessageType> messages)
        {
            serviceProvider.Publisher<TMessageType>().PublishRange(messages);
        }

        /// <summary>
        ///     Subscribes to a message using a message handler chain builder
        /// </summary>
        /// <typeparam name="TMessageType">
        ///     The message type
        /// </typeparam>
        /// <param name="serviceProvider">
        ///     The service provider
        /// </param>
        /// <param name="builderAction">The method used to configure the new subscription</param>
        /// <returns>
        ///     The <see cref="IChainBuilder{TMessageType}" /> used to setup the message handler chain.
        /// </returns>
        public static IChain<TMessageType> Subscribe<TMessageType>(this IServiceProvider serviceProvider, Action<IChainBuilder<TMessageType>> builderAction)
        {
            return serviceProvider.Subscriptions<TMessageType>().Subscribe(builderAction);
        }

        /// <summary>
        /// Gets the IMessageBusSubscriptions&lt;TMessageType&gt; from the service provider
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>The IMessageBusSubscriptions&lt;TMessageType&gt;</returns>
        public static IMessageBusSubscriptions<TMessageType> Subscriptions<TMessageType>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IMessageBusSubscriptions<TMessageType>>();
        }
    }
}