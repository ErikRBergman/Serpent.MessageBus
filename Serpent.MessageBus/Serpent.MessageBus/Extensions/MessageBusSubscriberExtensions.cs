// ReSharper disable CheckNamespace

namespace Serpent.MessageBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;
    using Serpent.Chain.Notification;

    /// <summary>
    /// The message bus subscriber extension types
    /// </summary>
    public static class MessageBusSubscriptionsExtensions
    {
        /// <summary>
        ///     Registers a subscription that through a factory creates a unique message handler for each message and then calls a
        ///     handler function
        ///     This functionality can also be replicated on your own by using:
        ///     subscriptions.Subscribe(message => { var handler = new HandlerClass(); return handler.HandleMessageAsync(message); },
        ///     null);
        /// </summary>
        /// <typeparam name="TMessage">The bus message type</typeparam>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="subscriptions">The message bus</param>
        /// <param name="messageHandlerFactoryFunc">The factory func (that creates the handler)</param>
        /// <param name="messageHandlerFactoryFuncSelector">The func that selectes the function to execute on the handler</param>
        /// <returns>A subscription wrapper</returns>
        public static IMessageBusSubscription RegisterFactory<TMessage, THandler>(
            this IMessageBusSubscriptions<TMessage> subscriptions,
            Func<THandler> messageHandlerFactoryFunc,
            Func<THandler, Func<TMessage, CancellationToken, Task>> messageHandlerFactoryFuncSelector)
        {
            return subscriptions.Subscribe(
                (message, token) =>
                    {
                        var handler = messageHandlerFactoryFunc();
                        var handlerFunc = messageHandlerFactoryFuncSelector(handler);
                        return handlerFunc(message, token);
                    });
        }

        /// <summary>
        ///     Create a message handler chain to set up a subscription
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="subscriptions">The subscriptions interface</param>
        /// <param name="setupAction">The method called to configure the message handler chain for the new subscription</param>
        /// <returns>The new message handler chain</returns>
        public static IChain<TMessageType> Subscribe<TMessageType>(
            this IMessageBusSubscriptions<TMessageType> subscriptions,
            Action<IChainBuilder<TMessageType>> setupAction)
        {
            var builder = new ChainBuilder<TMessageType>();
            setupAction(builder);

            var subscriptionNotification = new ChainBuilderNotifier();
            var services = new ChainBuilderSetupServices(subscriptionNotification);
            var chainFunc = builder.BuildFunc(services);

            var subscription = subscriptions.Subscribe(chainFunc);

            var chain = new Chain<TMessageType>(chainFunc, subscription.Dispose);
            subscriptionNotification.Notify(chain);

            return chain;
        }

        /// <summary>
        ///     Register a subscription that through a factory creates a unique message handler for each message, calls a handler
        ///     and then disposes the handler function
        ///     This functionality can also be replicated on your own by using:
        ///     subscriptions.Subscribe(async message => { var handler = new HandlerClass(); await
        ///     handler.HandleMessageAsync(message); handler.Dispose(); }, null);
        /// </summary>
        /// <typeparam name="TMessage">The bus message type</typeparam>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="subscriptions">The message bus</param>
        /// <param name="messageHandlerFactoryFunc">The factory func (that creates the handler)</param>
        /// <param name="messageHandlerFactoryFuncSelector">The func that selectes the function to execute on the handler</param>
        /// <returns>A subscription wrapper</returns>
        public static IMessageBusSubscription RegisterFactoryWithDisposableHandler<TMessage, THandler>(
            this IMessageBusSubscriptions<TMessage> subscriptions,
            Func<THandler> messageHandlerFactoryFunc,
            Func<THandler, Func<TMessage, CancellationToken, Task>> messageHandlerFactoryFuncSelector)
            where THandler : IDisposable
        {
            return subscriptions.Subscribe(
                async (message, token) =>
                    {
                        var handler = messageHandlerFactoryFunc();
                        var handlerFunc = messageHandlerFactoryFuncSelector(handler);
                        await handlerFunc(message, token).ConfigureAwait(false);
                        handler.Dispose();
                    });
        }

        /// <summary>
        /// Subscribes to a message bus
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="subscriptions">The subscriptions</param>
        /// <param name="handlerFunc">The handler method</param>
        /// <returns>A message bus subscription</returns>
        public static IMessageBusSubscription Subscribe<TMessageType>(this IMessageBusSubscriptions<TMessageType> subscriptions, Func<TMessageType, Task> handlerFunc)
        {
            return subscriptions.Subscribe((message, token) => handlerFunc(message));
        }
    }
}