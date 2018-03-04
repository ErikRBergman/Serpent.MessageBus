// ReSharper disable once CheckNamespace

namespace Serpent.MessageBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Concurrent message bus options extensions
    /// </summary>
    public static class BusOptionsExtensions
    {
        /// <summary>
        ///     Configures the bus to use a custom bus publisher
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <param name="customBusPublisher">The custom bus publisher</param>
        /// <returns>The options</returns>
        public static BusOptions<TMessageType> UseCustomPublisher<TMessageType>(
            this BusOptions<TMessageType> options,
            BusPublisher<TMessageType> customBusPublisher)
        {
            options.CustomPublishFunc = customBusPublisher.PublishAsync;
            return options;
        }

        /// <summary>
        ///     Configures the bus to use the forced parallel publisher. Every message subscription handler is executed on a newly
        ///     spawned Task
        /// </summary>
        /// <typeparam name="TMessageType">
        ///     The message type
        /// </typeparam>
        /// <param name="options">
        ///     The options
        /// </param>
        /// <returns>
        ///     The options
        /// </returns>
        public static BusOptions<TMessageType> UseForcedParallelPublisher<TMessageType>(this BusOptions<TMessageType> options)
        {
            options.UseCustomPublisher(ForcedParallelPublisher<TMessageType>.Default);
            return options;
        }

        /// <summary>
        ///     Use the serial publisher. Only a single subscription handler is executed at a time.
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <returns>The options</returns>
        public static BusOptions<TMessageType> UseSerialPublisher<TMessageType>(this BusOptions<TMessageType> options)
        {
            return options.UseCustomPublisher(SerialPublisher<TMessageType>.Default);
        }

        /// <summary>
        ///     Use a single receiver publisher
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <param name="customHandlerMethod">The custom </param>
        /// <returns>The options</returns>
        public static BusOptions<TMessageType> UseSingleReceiverPublisher<TMessageType>(
            this BusOptions<TMessageType> options,
            Func<Func<TMessageType, CancellationToken, Task>, TMessageType, CancellationToken, Task> customHandlerMethod = null)
        {
            return options.UseCustomPublisher(new SingleReceiverPublisher<TMessageType>(customHandlerMethod));
        }
    }
}